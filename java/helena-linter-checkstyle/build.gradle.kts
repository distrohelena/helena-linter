plugins {
    `java-library`
}

dependencies {
    implementation("com.puppycrawl.tools:checkstyle:${providers.gradleProperty("checkstyleVersion").get()}")

    testImplementation(platform("org.junit:junit-bom:${providers.gradleProperty("junitVersion").get()}"))
    testImplementation("org.junit.jupiter:junit-jupiter")
    testImplementation("com.puppycrawl.tools:checkstyle:${providers.gradleProperty("checkstyleVersion").get()}")
}

java {
    withSourcesJar()
    withJavadocJar()
}
