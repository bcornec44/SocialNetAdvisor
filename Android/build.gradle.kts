// build.gradle.kts (au niveau du projet)
buildscript {

    extra["compose_version"] = "1.4.3"

    repositories {
        google()
        mavenCentral()
    }

    dependencies {
        classpath("com.android.tools.build:gradle:8.0.2")
        classpath("org.jetbrains.kotlin:kotlin-gradle-plugin:1.8.21")
    }
}

plugins {
    id("org.jetbrains.kotlin.jvm") version "1.8.21"
}
