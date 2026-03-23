plugins {
    java
    checkstyle
}

val workspaceBuild = rootProject.extensions.extraProperties.has("helenaWorkspaceBuild")
val helenaCheckstyleConfig = file("config/checkstyle/checkstyle.xml")

repositories {
    mavenCentral()
}

dependencies {
    checkstyle("com.puppycrawl.tools:checkstyle:10.21.4")

    if (workspaceBuild) {
        checkstyle(project(":helena-linter-checkstyle"))
    } else {
        checkstyle(files("../helena-linter-checkstyle/build/libs/helena-linter-checkstyle-0.1.0.jar"))
    }
}

checkstyle {
    toolVersion = "10.21.4"
    configFile = helenaCheckstyleConfig
}

tasks.named("checkstyleMain") {
    onlyIf {
        gradle.startParameter.taskNames.any { taskName -> taskName.endsWith("checkstyleMain") }
    }
}

if (!workspaceBuild) {
    tasks.named("checkstyleMain") {
        dependsOn(gradle.includedBuild("helena-linter-checkstyle").task(":jar"))
    }
}
