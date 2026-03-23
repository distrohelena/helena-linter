import org.gradle.api.plugins.JavaPluginExtension
import org.gradle.api.tasks.testing.Test
import org.gradle.jvm.toolchain.JavaLanguageVersion

plugins {
    base
}

extra["helenaWorkspaceBuild"] = true

val javaVersion = providers.gradleProperty("javaVersion").get().toInt()
val projectGroup = providers.gradleProperty("projectGroup").get()
val projectVersion = providers.gradleProperty("projectVersion").get()

subprojects {
    group = projectGroup
    version = projectVersion

    repositories {
        mavenCentral()
    }

    plugins.withId("java") {
        extensions.configure(JavaPluginExtension::class.java) {
            toolchain.languageVersion.set(JavaLanguageVersion.of(javaVersion))
        }

        tasks.withType(Test::class.java).configureEach {
            useJUnitPlatform()
        }
    }
}
