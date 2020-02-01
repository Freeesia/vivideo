pipeline{
  agent{
    docker {
      label 'docker'
      image 'mcr.microsoft.com/dotnet/core/sdk:3.1'
    }
  }
  options {
    timestamps()
  }
  environment {
    DOTNET_CLI_HOME = "/tmp/DOTNET_CLI_HOME"
  }
  stages{
    stage("setup"){
      steps{
        sh 'dotnet tool restore'
      }
    }
    stage("build"){
      steps{
        script {
          text = sh (returnStdout: true, script: 'dotnet gitversion -nofetch')
          print text
          json = readJSON(text:text)
          sh """
          dotnet publish -c Release --self-contained -r win10-x64 -o out \
          -p:Version=$json.FullSemVer \
          -p:AssemblyVersion=$json.AssemblySemVer \
          -p:FileVersion=$json.AssemblySemFileVer \
          -p:InformationalVersion=$json.InformationalVersion \
          Worker
          """
        }
      }
    }
    stage("archive"){
      steps{
        sh 'dotnet script eval \'System.IO.Compression.ZipFile.CreateFromDirectory("out", "out.zip");\''
        archiveArtifacts artifacts: 'out.zip', fingerprint: true, onlyIfSuccessful: true
      }
    }
  }
  // post {
  //   failure {
  //     script {
  //       def slackDomain = 'rokkyapp'
  //       def slackCh = '#git_notification'
  //       def slackToken = '2lTyhICh8FVv8KFfoatYdxfp'
  //       def msg = "<@UA173K1CG> スクレイプ失敗 - ${env.JOB_NAME} ${env.BUILD_NUMBER} (<${env.BUILD_URL}console|ログ>)"
  //       slackSend teamDomain: slackDomain, channel: slackCh, token: slackToken, color: 'danger', message: msg
  //     }
  //   }
  // }
}
