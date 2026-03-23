plugins {
    `java-library`
}

group = "dev.distrohelena"
version = "0.1.0"

val checkstyleVersion = "10.21.4"
val junitVersion = "5.11.4"

repositories {
    mavenCentral()
}

dependencies {
    implementation("com.puppycrawl.tools:checkstyle:$checkstyleVersion")

    testImplementation(platform("org.junit:junit-bom:$junitVersion"))
    testImplementation("org.junit.jupiter:junit-jupiter")
    testImplementation("com.puppycrawl.tools:checkstyle:$checkstyleVersion")
}

java {
    withSourcesJar()
    withJavadocJar()
}
