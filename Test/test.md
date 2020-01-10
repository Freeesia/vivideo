
```
ffmpeg\bin\ffmpeg -i raw\video.mp4 -c:v h264_qsv -q:v 20 -c:a copy -f hls -hls_time 9 -hls_playlist_type vod -hls_segment_filename "out\video%3d.ts" out\video.m3u8
npm install http-server -g
npx http-server . -gb --cors
```
qsvはIntel GPUがプライマリじゃないとffmpegで使えないらしい