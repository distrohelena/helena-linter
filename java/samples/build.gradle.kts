plugins {
    java
    checkstyle
}

val workspaceBuild = rootProject.extensions.extraProperties.has("helenaWorkspaceBuild")

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
    configFile = if (workspaceBuild) {
        rootProject.file("helena-linter-checkstyle/src/main/resources/helena_checks.xml")
    } else {
        rootProject.file("../helena-linter-checkstyle/src/main/resources/helena_checks.xml")
    }
}

if (!workspaceBuild) {
    tasks.named("checkstyleMain") {
        dependsOn(gradle.includedBuild("helena-linter-checkstyle").task(":jar"))
    }
}
