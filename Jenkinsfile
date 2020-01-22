pipeline{
  agent{
    docker { image 'mcr.microsoft.com/dotnet/core/sdk:3.1' }
  }
  options {
    timestamps()
  }
  stages{
    stage("build"){
      steps{
        sh '''
        dotnet tool restore
        dotnet publish -c Release --self-contained -r win10-x64 -o out Worker
        dotnet cszip out/ out.zip
        '''
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
