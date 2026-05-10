# CaptureSys

`Assets/Game/CaptureSys` 提供一套开箱即用的 2D 拍照系统，适合直接作为 `Asset Package` 导出。

## Asset 包说明

- 核心运行时代码不再依赖项目内的 `Assets/InputSystem_Actions.inputactions`。
- `CaptureCamera.prefab` 默认不绑定外部 `InputActionAsset`。
- 导入到其他 Unity 项目后，只要目标项目安装了 `Input System` 包，就可以直接使用。
- 运行时默认按键：
`E` 拍照
`F` 清空已保存的照片和索引

## 用法

1. 把 [CaptureCamera.prefab](/D:/GameDev/Unity/Projects/Memory-Album/Assets/Game/CaptureSys/Prefabs/CaptureCamera.prefab) 拖进场景。
2. 在需要被拍摄的物体上挂 `CaptureObj`，填写 `Object Id`。
3. 运行后按 `E` 拍照。
4. 运行后按 `F` 清空已保存的相片和 `captures.json` 索引。

如果你已经有自己的输入表，也可以把 `CaptureCameraController` 上的 `Input Actions Asset` 指向你自己的 `InputActionAsset`，再配置：

- `Action Map Name`：默认 `Player`
- `Action Name`：默认 `Capture`
- `Clear Action Name`：默认 `ClearCapturePhotos`

## 导出 Asset 包建议

建议导出整个 [CaptureSys](/D:/GameDev/Unity/Projects/Memory-Album/Assets/Game/CaptureSys) 目录：

- `Runtime/`
- `Prefabs/`
- `Examples/`
- `Tests/`
- `README.md`

如果只想给其他项目使用核心功能，最少导出：

- `Runtime/`
- `Prefabs/CaptureCamera.prefab`

## 示例资源

- 示例场景：[CaptureDemoScene.unity](/D:/GameDev/Unity/Projects/Memory-Album/Assets/Game/CaptureSys/Examples/Scenes/CaptureDemoScene.unity)
- 示例目标预制体：[DemoCaptureTarget.prefab](/D:/GameDev/Unity/Projects/Memory-Album/Assets/Game/CaptureSys/Examples/Prefabs/DemoCaptureTarget.prefab)
- 示例遮挡物预制体：[DemoOccluder.prefab](/D:/GameDev/Unity/Projects/Memory-Album/Assets/Game/CaptureSys/Examples/Prefabs/DemoOccluder.prefab)

打开示例场景后：

- 按 `E` 会尝试拍照。
- 按 `F` 会清空当前已经保存到本地的照片和索引。
- 左侧 `Visible Capture Target` 会被识别并写入照片记录。
- 右侧 `Hidden Capture Target` 会被 `Demo Occluder` 挡住，不会进入照片记录。

## 行为

- 只有当相机视野范围内检测到至少一个可见 `CaptureObj` 时才会真正保存图片。
- 如果没有检测到可拍物体，会输出 `没有检测到CaptureObj`。
- 清空相片时，会删除 `Application.persistentDataPath/CaptureSys/Photos/` 下已保存的 PNG，并重置拍照索引。

## 保存位置

- 默认根目录：`Application.persistentDataPath/CaptureSys/`
- 当前项目在 Windows 下通常对应：
`C:\Users\Administrator\AppData\LocalLow\DefaultCompany\Memory Album\CaptureSys\`
- 图片目录：
`C:\Users\Administrator\AppData\LocalLow\DefaultCompany\Memory Album\CaptureSys\Photos\`
- 索引文件：
`C:\Users\Administrator\AppData\LocalLow\DefaultCompany\Memory Album\CaptureSys\captures.json`

## JSON 结构

`captures.json` 会记录以下内容：

- `nextSequence`
下一张照片可用的自增序号。每成功拍一张后会递增，用于生成类似 `capture_0001.png` 的文件名。

- `photos`
照片记录列表。每个元素代表一张已经成功保存到本地的照片。

- `photos[].sequence`
这张照片自己的序号，和生成的图片文件名一一对应。

- `photos[].imageFileName`
本地图片文件名，例如 `capture_0001.png`。后续读取相册时可以用它去 `Photos` 目录检索真实图片。

- `photos[].capturedAtUtc`
拍照时间，使用 UTC 时间字符串保存。便于排序、调试和后续做时间展示。

- `photos[].capturedObjects`
这张照片里识别到的所有 `CaptureObj` 记录列表。

- `photos[].capturedObjects[].objectId`
被拍到物体的字符串唯一标识，对应 `CaptureObj` 组件上配置的 `Object Id`。

- `photos[].capturedObjects[].viewportOffset`
物体相对相机中心点的视口偏移量。是归一化坐标偏移，不是世界坐标。
例如：
`(0, 0)` 表示在画面中心，
`(0.5, 0)` 表示在画面右侧一半位置，
`(-0.5, 0.5)` 表示在画面左上区域。

- `photos[].capturedObjects[].distanceFromCenter`
物体到画面中心的二维距离，基于 `viewportOffset` 计算。数值越小越接近画面中心，越大越靠近边缘。

## 备注

- 系统默认使用正交相机的 `orthographicSize` 和 `aspect` 计算当前视野范围。
- 遮挡判定基于当前视线方向做检测，不是朝相机中心点发射射线。
- 2D 场景优先使用 `Physics2D`，如果目标或遮挡物使用 3D Collider，也会补充使用 `Physics` 做判定。
- 为避免和主相机冲突，拍照相机预制体默认不额外挂 `AudioListener`。
