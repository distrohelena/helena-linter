plugins {
    java
    checkstyle
}

dependencies {
    checkstyle(project(":helena-linter-checkstyle"))
}

checkstyle {
    toolVersion = providers.gradleProperty("checkstyleVersion").get()
    configFile = rootProject.file("helena-linter-checkstyle/src/main/resources/helena_checks.xml")
}
