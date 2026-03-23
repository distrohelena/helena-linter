import org.gradle.api.plugins.JavaPluginExtension
import org.gradle.api.tasks.testing.Test
import org.gradle.jvm.toolchain.JavaLanguageVersion

plugins {
    base
}

val javaVersion = providers.gradleProperty("javaVersion").get().toInt()

subprojects {
    group = "dev.distrohelena"
    version = "0.1.0"

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
