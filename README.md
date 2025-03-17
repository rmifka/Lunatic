
---

# 🎮 Lunatic Modding Framework

Welcome to **Lunatic** — a **super lightweight, just-magic modding framework** for Unity games!  
If you’re tired of overcomplicated mod frameworks and just want something clean and simple, this is for you.  

---

## ✨ What is Lunatic?

Lunatic is a small but powerful system that lets you **load mods into Unity games** with **zero bloat**.  
It has a modular design so you only get what you need — no weird magic, just straight-up mod loading.

---

## 🚀 Features

- 📦 **Simple Loader** for loading and running mods.
- ⚙️ **Bootstrapper** that hooks cleanly into Unity games.
- 💬 **Public API** for mod developers to use.
- 🧩 **Harmony** support for patching game methods.
- 🖥️ **Modern installer GUI** made with Avalonia + SukiUI.
- 🔥 Focused on **performance, simplicity, and transparency**.

---

## 🛠 How Does It Work?

1. Use the **Installer App** to set up Lunatic in your game folder.
2. Lunatic’s **Bootstrapper** gets injected and kicks things off.
3. The **Loader** looks for mods and fires them up.
4. Mods do their thing — like adding new features or patches.

---

## 📂 How It's Organized

| Project                   | What it does                                   |
|--------------------------|------------------------------------------------|
| `Lunatic.Installer`      | The nice-looking app to install/uninstall mods.|
| `Lunatic.Bootstrapper`   | Hooks into the game to start the Loader.       |
| `Lunatic.Loader`         | Loads mods and libraries, applies Harmony patches.|
| `Lunatic.Public`         | The public API mod developers will use.        |

---

## 🧑‍💻 Making Your First Mod

1. Reference `Lunatic.Public` in your mod project.
2. Create a class that implements `IModule`.
3. Example:

```csharp
public class MyCoolMod : IModule
{
    public void Initialize(ILogger logger)
    {
        logger.Info("Hello from MyCoolMod!");
    }
}
```

4. Drop your compiled `.dll` into the game’s `Mods` folder. That's it!

There is a comprehensive TestModule in this repo.
https://github.com/rmifka/Lunatic/blob/96d07e0e44c9c66a0dad0368d05f23c99230363e/Lunatic.TestModule/Module.cs#L6-L16

---

## 💻 Installing Lunatic

1. Run **Lunatic.Installer**.
2. Pick your game folder.
3. Hit **Install Lunatic**.
4. Done! Mods will load when you start the game.

---

## 🧹 Uninstalling Lunatic

- Run the **Installer**, choose your game, and hit **Uninstall Lunatic**.  
- It’ll clean up and restore backups.

---

## ⚙️ Dependencies

- **.NET Standard 2.1**
- **Harmony 2.3.x+ (Fat version)** for patching.
- **Avalonia + SukiUI** for the fancy installer app.

---

## 💡 Why Lunatic?

- No complicated setup.
- Easy to use and extend.
- Minimal and fast.
- Actually **fun** to work with.

---

## 🚧 Contributing

If you have ideas or want to improve Lunatic, **PRs and issues are always welcome**!  
Let’s keep it **simple and clean**, though — no unnecessary complexity.

---

## 📜 License

MIT. Do whatever you want, just don’t sue me.

---

## ❤️ Credits

Originally made by Renschi with love for the Unity modding community.  
Big thanks to Harmony, Avalonia, and SukiUI for making this possible!

---
