# Comprehensive Game Engine Tutorial

This tutorial walks you through building and running our custom 2D game engine, written in F# and compiled to JavaScript via Fable. You’ll learn how to manage sprites, handle input, simulate physics, control the camera, manage scenes, and play sounds—all in the browser.

---

## Table of Contents

1. [Introduction](#introduction)
2. [Project Setup](#project-setup)
3. [Configuration Files](#configuration-files)
4. [Sprite Configuration](#sprite-configuration)
5. [Main Game File (`src/Main.fs`)](#main-game-file-srcmainfs)
6. [Entry Point (`src/entry.fs`)](#entry-point-srcentryfs)
7. [Using Engine Components](#using-engine-components)

   * [Sprites](#71-sprites)
   * [Input Handling](#72-input-handling)
   * [Physics](#73-physics)
   * [Camera](#74-camera)
   * [Scene Management](#75-scene-management)
   * [Sound Management](#76-sound-management)
8. [Animation](#animation)
9. [Running Your Game](#running-your-game)
10. [Building for Production](#building-for-production)

---

## 1. Introduction

Our engine provides a lightweight framework for 2D browser games, featuring:

* **Sprite management** via JSON-defined atlases
* **Input handling** (keyboard, mouse)
* **Physics simulation** (gravity, collisions, landing)
* **Camera control** (panning, zoom)
* **Scene rendering** with WebGL
* **Sound management** (music & SFX)

---

## 2. Project Setup

1. **Prerequisites**

   * [Node.js](https://nodejs.org/) & npm
   * .NET SDK (for F# tooling)

2. **Clone & Install**

   ```bash
   git clone <your-repo-url>
   cd project_root
   npm install
   ```

3. **Directory Layout**

   ```
   project_root/
   ├── src/
   │   ├── Engine/
   │   │   ├── Camera.fs
   │   │   ├── GameLoop.fs
   │   │   ├── GetSpriteData.fs
   │   │   ├── Input.fs
   │   │   ├── Physics.fs
   │   │   ├── Runtime.fs
   │   │   ├── Scene.fs
   │   │   ├── Sprite.fs
   │   │   └── SpriteAtlas.fs
   │   ├── Main.fs
   │   └── entry.fs
   ├── public/
   │   ├── index.html
   │   ├── sprites.json
   │   └── [asset files]
   ├── package.json
   ├── webpack.config.js
   └── fengine.fsproj
   ```

---

## 3. Configuration Files

### `webpack.config.js`

```js
const path = require("path");

module.exports = {
  entry: "./src/entry.fs.js",
  output: {
    path: path.join(__dirname, "./public"),
    filename: "bundle.js",
  },
  devServer: {
    static: path.join(__dirname, "./public"),
  },
  module: {
    rules: [
      {
        test: /\.fs(x|proj)?$/,
        use: "fable-loader"
      }
    ]
  }
};
```

### `fengine.fsproj`

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <ItemGroup>
    <Compile Include="src/Engine/GetSpriteData.fs" />
    <Compile Include="src/Engine/Input.fs" />
    <Compile Include="src/Engine/Sprite.fs" />
    <Compile Include="src/Engine/Runtime.fs" />
    <Compile Include="src/Engine/Camera.fs" />
    <Compile Include="src/Engine/Physics.fs" />
    <Compile Include="src/Engine/Scene.fs" />
    <Compile Include="src/Engine/GameLoop.fs" />
    <Compile Include="src/Main.fs" />
    <Compile Include="src/entry.fs" />
  </ItemGroup>
</Project>
```

---

## 4. Sprite Configuration

Create `public/sprites.json` to define your sprite sheets:

```json
{
  "player.png": [
    {
      "startx": 0,
      "starty": 0,
      "width": 32,
      "height": 32,
      "col": 4,
      "row": 1,
      "animation": true,
      "no": 0
    }
  ],
  "platform.png": [
    {
      "startx": 0,
      "starty": 0,
      "width": 64,
      "height": 16,
      "col": 1,
      "row": 1,
      "animation": false,
      "no": 0
    }
  ]
}
```

* `"col"` = frames per row
* `"row"` = number of rows
* `"animation": true` enables frame-based animation

---

## 5. Main Game File (`src/Main.fs`)

```fsharp
module Main

open Engine.GameLoop
open Engine.Sprite
open Engine.Scene
open Engine.Camera
open Engine.Physics
open Engine.Input
open Engine.SoundManager

let mutable player    = Unchecked.defaultof<Sprite>
let mutable platforms = []
let mutable camera    = Unchecked.defaultof<Camera>

let init () =
    // Player
    player <- Sprite.create "player.png" (50.0, 50.0) (32.0, 32.0)
    player.affectedByGravity <- true

    // Platforms
    platforms <- [
      Sprite.create "platform.png" (0.0, 100.0) (200.0, 20.0)
      Sprite.create "platform.png" (250.0, 150.0) (200.0, 20.0)
    ]

    // Camera & background
    camera <- Camera.create 0.0 0.0 800.0 600.0
    Scene.setBackgroundColor 0.5 0.7 1.0 1.0

    // Input
    Input.onKeyDown (fun key ->
      match key with
      | "ArrowLeft"  -> player.velocityX <- -5.0
      | "ArrowRight" -> player.velocityX <-  5.0
      | "Space" when player.isGrounded ->
          player.velocityY <- -10.0
          SoundManager.playSound "jump" None
      | _ -> ()
    )
    Input.onKeyUp (fun key ->
      match key with
      | "ArrowLeft" | "ArrowRight" -> player.velocityX <- 0.0
      | _ -> ()
    )

    // Sounds
    SoundManager.loadSound "backgroundMusic" "music.mp3" true
    SoundManager.loadSound "jump"            "jump.mp3"     false
    SoundManager.playSound "backgroundMusic" (Some true)

    // Animation
    Sprite.startSpriteAnimation player

let update (dt: float) =
    Physics.applyGravity player
    Physics.updatePosition player
    platforms |> List.iter (fun p -> Physics.handlePlatformLanding player p)
    Camera.centerOn camera player.x player.y

let draw (dt: float) =
    Scene.draw camera dt

let startGame () =
    init()
    GameLoop.onUpdate update
    GameLoop.onDraw   draw
    GameLoop.start()

startGame()
```

---

## 6. Entry Point (`src/entry.fs`)

```fsharp
module Entry
open Main
// Game boots automatically via Main.startGame()
```

---

## 7. Using Engine Components

### 7.1 Sprites

```fsharp
let enemy = Sprite.create "enemy.png" (200.0, 100.0) (32.0, 32.0)
enemy.flipX <- true
enemy.scale <- 1.2
```

### 7.2 Input Handling

```fsharp
Input.onKeyDown (fun key ->
  if key = "Escape" then GameLoop.stop()
)
```

### 7.3 Physics

```fsharp
Physics.applyGravity player
Physics.updatePosition player
Physics.handlePlatformLanding player platform
```

### 7.4 Camera

```fsharp
let cam = Camera.create 0.0 0.0 800.0 600.0
Camera.centerOn cam player.x player.y
Camera.zoom    cam 1.5
```

### 7.5 Scene Management

```fsharp
Scene.setBackgroundColor 0.1 0.1 0.1 1.0
Scene.draw camera dt
```

### 7.6 Sound Management

```fsharp
SoundManager.loadSound "bgm" "theme.mp3" true
SoundManager.playSound "bgm" (Some true)
SoundManager.setMasterVolume 0.8
```

---

## 8. Animation

1. **Prepare** a sprite sheet with sequential frames.
2. **Configure** in `sprites.json`:

   ```json
   {
     "player_walk.png": [
       {
         "startx": 0, "starty": 0,
         "width": 32, "height": 32,
         "col": 8, "row": 1,
         "animation": true, "no": 0
       }
     ]
   }
   ```
3. **Create** & **control**:

   ```fsharp
   let walker = Sprite.create "player_walk.png" pos size
   Sprite.startSpriteAnimation walker
   Sprite.stopSpriteAnimation  walker
   Scene.setDefaultAnimationFrameDuration 0.1
   walker.no <- 1  // switch row for alternate animation
   ```

---

## 9. Running Your Game

```bash
npm start
```

Open [http://localhost:8080](http://localhost:8080) in your browser.

---

## 10. Building for Production

1. **Add** to `package.json`:

   ```json
   "scripts": {
     "build": "webpack --mode production"
   }
   ```
2. **Run**:

   ```bash
   npm run build
   ```

Your optimized `bundle.js` will be in `public/`—ready for deployment!

