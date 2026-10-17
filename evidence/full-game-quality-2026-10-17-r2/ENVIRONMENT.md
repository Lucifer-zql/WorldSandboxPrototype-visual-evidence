# 环境与复现

- OS: Windows
- Unity: 6000.6.5f1 (batchmode)
- Project: WorldSandboxPrototype @ f85fbe3 + 本轮工作区修改（主仓库未提交/未推送）
- 截图: Assets/Tests/PlayMode/D068EvidenceCaptureTests.cs，环境变量 D068_SHOT_DIR
- 存档: WS_SAVE_DIR 指向临时目录

复现命令（示例）：

    Unity.exe -batchmode -projectPath <repo> -runTests -testPlatform EditMode \
      -testResults tests/editmode-full.xml -logFile tests/editmode-full.log

    $env:D068_SHOT_DIR='<out>'; Unity.exe -batchmode -projectPath <repo> -runTests -testPlatform PlayMode \
      -testFilter D068 -testResults tests/playmode-d068-full.xml

    Unity.exe -batchmode -projectPath <repo> -runTests -testPlatform PlayMode \
      -testFilter D067 -testResults tests/playmode-d067-full.xml
