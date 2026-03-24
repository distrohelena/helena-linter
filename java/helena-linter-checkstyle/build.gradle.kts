import java.net.HttpURLConnection
import java.net.URI
import java.nio.charset.StandardCharsets
import java.util.Base64

plugins {
    `java-library`
    `maven-publish`
    signing
}

val checkstyleVersion = "10.21.4"
val junitVersion = "5.11.4"
val centralPortalBaseUrl = "https://ossrh-staging-api.central.sonatype.com"

fun propertyOrEnv(propertyName: String, envName: String) =
    providers.gradleProperty(propertyName).orElse(providers.environmentVariable(envName))

val centralPortalUsername = propertyOrEnv("centralPortalUsername", "CENTRAL_PORTAL_USERNAME")
val centralPortalPassword = propertyOrEnv("centralPortalPassword", "CENTRAL_PORTAL_PASSWORD")
val centralPublishingType = providers.gradleProperty("centralPublishingType").orElse("automatic")
val signingKey = propertyOrEnv("signingKey", "SIGNING_KEY")
val signingKeyFile = propertyOrEnv("signingKeyFile", "SIGNING_KEY_FILE")
val signingKeyId = propertyOrEnv("signingKeyId", "SIGNING_KEY_ID")
val signingPassword = propertyOrEnv("signingPassword", "SIGNING_PASSWORD")

repositories {
    mavenCentral()
}

dependencies {
    compileOnly("com.puppycrawl.tools:checkstyle:$checkstyleVersion")

    testImplementation(platform("org.junit:junit-bom:$junitVersion"))
    testImplementation("org.junit.jupiter:junit-jupiter")
    testImplementation("com.puppycrawl.tools:checkstyle:$checkstyleVersion")
}

java {
    withSourcesJar()
    withJavadocJar()
}

publishing {
    publications {
        create<MavenPublication>("mavenJava") {
            from(components["java"])

            artifactId = "helena-linter-checkstyle"

            pom {
                name.set("Helena Linter Checkstyle")
                description.set("Helena readability and control-flow Checkstyle rules for Java projects.")
                url.set("https://github.com/distrohelena/helena-linter")

                licenses {
                    license {
                        name.set("MIT")
                        url.set("https://github.com/distrohelena/helena-linter/blob/master/LICENSE")
                    }
                }

                developers {
                    developer {
                        id.set("distrohelena")
                        name.set("DistroHelena")
                    }
                }

                scm {
                    url.set("https://github.com/distrohelena/helena-linter")
                    connection.set("scm:git:https://github.com/distrohelena/helena-linter.git")
                    developerConnection.set("scm:git:ssh://git@github.com/distrohelena/helena-linter.git")
                }
            }
        }
    }

    repositories {
        maven {
            name = "ossrhStagingApi"
            url = URI("$centralPortalBaseUrl/service/local/staging/deploy/maven2/")

            credentials {
                username = centralPortalUsername.orNull
                password = centralPortalPassword.orNull
            }
        }
    }
}

signing {
    val configuredSigningKey =
        signingKey.orNull ?: signingKeyFile.orNull?.let { project.file(it).readText() }
    val configuredSigningPassword = signingPassword.orNull

    if (configuredSigningKey != null && configuredSigningPassword != null) {
        val configuredSigningKeyId = signingKeyId.orNull

        if (configuredSigningKeyId != null) {
            useInMemoryPgpKeys(configuredSigningKeyId, configuredSigningKey, configuredSigningPassword)
        } else {
            useInMemoryPgpKeys(configuredSigningKey, configuredSigningPassword)
        }

        sign(publishing.publications["mavenJava"])
    }
}

val checkCentralPublishEnvironment = tasks.register("checkCentralPublishEnvironment") {
    group = "publishing"
    description = "Validates the credentials and signing inputs required for Maven Central publishing."

    doLast {
        val missingItems = buildList {
            if (centralPortalUsername.orNull.isNullOrBlank()) {
                add("centralPortalUsername or CENTRAL_PORTAL_USERNAME")
            }
            if (centralPortalPassword.orNull.isNullOrBlank()) {
                add("centralPortalPassword or CENTRAL_PORTAL_PASSWORD")
            }
            if (signingKey.orNull.isNullOrBlank() && signingKeyFile.orNull.isNullOrBlank()) {
                add("signingKey, SIGNING_KEY, signingKeyFile, or SIGNING_KEY_FILE")
            }
            if (signingPassword.orNull.isNullOrBlank()) {
                add("signingPassword or SIGNING_PASSWORD")
            }
        }

        if (missingItems.isNotEmpty()) {
            error(
                "Missing required Maven Central publishing inputs:\n" +
                    missingItems.joinToString(separator = "\n") { "- $it" }
            )
        }
    }
}

tasks.register("publishToSonatypeCentral") {
    group = "publishing"
    description = "Publishes the signed mavenJava artifacts and requests release through Sonatype Central Portal."

    dependsOn(checkCentralPublishEnvironment)
    dependsOn("publishMavenJavaPublicationToOssrhStagingApiRepository")

    doLast {
        val namespace = project.group.toString()
        val publishingType = centralPublishingType.get()
        val authToken =
            Base64.getEncoder()
                .encodeToString(
                    "${centralPortalUsername.get()}:${centralPortalPassword.get()}".toByteArray(StandardCharsets.UTF_8)
                )
        val endpoint =
            URI(
                "$centralPortalBaseUrl/service/local/staging/manual/upload/defaultRepository/" +
                    "$namespace?publishing_type=$publishingType"
            ).toURL()
        val connection = endpoint.openConnection() as HttpURLConnection

        connection.requestMethod = "POST"
        connection.setRequestProperty("Authorization", "Bearer $authToken")
        connection.setRequestProperty("Accept", "application/json")
        connection.doOutput = true

        val responseCode = connection.responseCode
        val responseBody =
            (if (responseCode in 200..299) connection.inputStream else connection.errorStream)
                ?.bufferedReader()
                ?.use { it.readText() }
                .orEmpty()

        if (responseCode !in 200..299) {
            error(
                "Sonatype Central upload request failed with HTTP $responseCode.\n$responseBody"
            )
        }

        logger.lifecycle("Sonatype Central accepted the deployment for namespace $namespace.")
        if (responseBody.isNotBlank()) {
            logger.lifecycle(responseBody)
        }
    }
}
