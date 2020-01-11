
```
npm install http-server -g
npx http-server . -gb --cors
```

## CPUエンコード
### docker
```
docker run -it --rm -v $(pwd):$(pwd) -w $(pwd) jrottenberg/ffmpeg:alpine
```
### libx264
```
-i raw/video.mp4 -c:v libx264 -q:v 20 -c:a copy
```
## GPUエンコード
### docker
win10の仮想環境にGPUを設定出来ないので、dockerではGPUを使えない
```
docker run -it --rm --device /dev/dri:/dev/dri -v $(pwd):$(pwd) -w $(pwd) jrottenberg/ffmpeg:vaapi
```
### win
qsvはIntel GPUがプライマリじゃないとffmpegで使えないらしい
```
-i raw/video.mp4 -c:v h264_qsv -q:v 20 -c:a copy 
```
h265はGF980でデコード出来ない
```
-i raw/video.mp4 -c:v nvenc_h264 -q:v 20 -c:a copy
```
### mac
```
-i raw/video.mp4 -c:v h264_videotoolbox -q:v 20 -c:a copy
```
### linux(docker)
```
-i raw/video.mp4 -c:v h264_vaapi -q:v 20 -c:a copy
```

## HLS
```
-f hls -hls_time 9 -hls_playlist_type vod -hls_segment_filename "out/video%3d.ts" out/video.m3u8
```