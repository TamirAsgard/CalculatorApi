pipeline {
    agent any

    environment {
        TEST_PROJECT = 'src/IO.Swagger/IO.Swagger.csproj'
        TEST_DIR = 'IO.Swagger.Tests'
    }

    stages {
        stage('Checkout') {
            steps {
                checkout scm
            }
        }

        stage('Restore') {
            steps {
                sh 'dotnet restore'
            }
        }

        stage('Build') {
            steps {
                sh 'dotnet build --no-restore'
            }
        }

        stage('Test') {
            steps {
                sh 'dotnet test --no-build --logger "junit;LogFilePath=test-results.xml"'
            }
            post {
                always {
                    junit '**/test-results.xml'
                }
            }
        }

        stage('Push to Main') {
            when {
                // Run this stage only if the previous stages were successful
                // AND we are NOT already on the main branch (avoid loops)
                expression {
                    return currentBuild.result == null || currentBuild.result == 'SUCCESS' && env.BRANCH_NAME != 'main'
                }
            }
            steps {
                script {
                    // Configure git for the push
                    sh 'git config --global user.email "tamir303@gmail.com"'
                    sh 'git config --global user.name "tamir303"'
                    
                    // Fetch all branches
                    sh 'git fetch origin'
                    
                    // Check out main
                    sh 'git checkout main'
                    sh 'git pull origin main'
                    
                    // Merge the current branch (source of this build) into main
                    // Assuming env.BRANCH_NAME is available (Multibranch pipeline)
                    sh "git merge origin/${env.BRANCH_NAME} --no-ff -m 'Merge branch ${env.BRANCH_NAME} into main after successful tests'"
                    
                    // Push to main
                    withCredentials([usernamePassword(credentialsId: 'git-credentials-id', passwordVariable: 'GIT_PASSWORD', usernameVariable: 'GIT_USERNAME')]) {
                        sh 'git push origin main'
                    }
                }
            }
        }
    }
}
