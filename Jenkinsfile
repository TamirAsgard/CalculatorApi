pipeline {
    agent any

    environment {
        COMPOSE_FILE = 'docker-compose.test.yml'
    }

    stages {
        stage('Test') {
            steps {
                script {
                    try {
                        // Ensure clean state
                        sh "docker-compose -f ${COMPOSE_FILE} down -v"
                        
                        // Run tests
                        // --abort-on-container-exit ensures we get the exit code from the test runner
                        sh "docker-compose -f ${COMPOSE_FILE} up --build --abort-on-container-exit"
                    } catch (Exception e) {
                        currentBuild.result = 'FAILURE'
                        error("Tests failed")
                    } finally {
                        // Clean up
                        sh "docker-compose -f ${COMPOSE_FILE} down -v"
                    }
                }
            }
        }
    }
}
