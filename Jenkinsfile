pipeline {

    agent {

        docker { 
    
            image 'mydotnet_sdk:v0'
            args '--network jenkins"'

        }
    }

    stages {

        stage('Restore') {
            steps {

                sh 'dotnet restore'

            }
        }
         
        stage('Build') {
            steps {

                sh 'dotnet sonarscanner begin /k:"dotnet_client" /d:sonar.host.url="http://sonarqube:9000" /d:sonar.login="<SONARQUBE_LOGIN>"'

                sh 'dotnet build --no-restore'
            
                sh 'dotnet sonarscanner end /d:sonar.login="<SONARQUBE_LOGIN>"'

            }
        }

        stage('Deploy') {
            steps {
                
                sh 'rm -R -f deploy'
                sh 'mkdir deploy'

                // https://docs.microsoft.com/en-us/dotnet/core/rid-catalog

                // Alpine Linux
                sh 'dotnet publish --no-self-contained --runtime linux-musl-x64 -c Release ./dotnet_client.csproj -o ./deploy/linux-musl-x64'

                // Ubuntu, Debian, Fedora, CentOS
                sh 'dotnet publish --no-self-contained --runtime linux-x64 -c Release ./dotnet_client.csproj -o ./deploy/linux-x64'

                // Windows 10, Windows Server 2016
                sh 'dotnet publish --no-self-contained --runtime win10-x64 -c Release ./dotnet_client.csproj -o ./deploy/win10-x64'

                // macOS
                sh 'dotnet publish --no-self-contained --runtime osx-x64 -c Release ./dotnet_client.csproj -o ./deploy/osx-x64'
            }
        }
    }
}
