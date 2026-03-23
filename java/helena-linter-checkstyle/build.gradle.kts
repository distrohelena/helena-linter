plugins {
    `java-library`
    `maven-publish`
}

val checkstyleVersion = "10.21.4"
val junitVersion = "5.11.4"

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
}
