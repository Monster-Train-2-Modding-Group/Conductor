# Conductor

[![GitHub Release](https://img.shields.io/github/v/release/Monster-Train-2-Modding-Group/Conductor?color=4CAF50&label=latest)](https://github.com/Monster-Train-2-Modding-Group/Conductor/releases)
[![Trainworks Reloaded](https://img.shields.io/badge/framework-Trainworks--Reloaded-blue?logo=github)](https://github.com/Monster-Train-2-Modding-Group/Trainworks-Reloaded)
[![License](https://img.shields.io/github/license/Monster-Train-2-Modding-Group/Conductor?color=lightgrey)](https://github.com/Monster-Train-2-Modding-Group/Conductor/blob/main/LICENSE)

---

### Overview

**Conductor** is a modding library for **Monster Train 2**, providing a growing collection of reusable effects, status effects, triggers, and keywords.  
It’s designed to make modding **safer, cleaner, and easier** by abstracting away repetitive or risky code.

Conductor builds on top of [**Trainworks-Reloaded**](https://github.com/Monster-Train-2-Modding-Group/Trainworks-Reloaded) and adds additional utilities, APIs, and hooks for advanced mod development.

---

## Installation

### For Players

If you install a mod that depends on **Conductor**, it will automatically be downloaded by your mod manager (e.g. r2modman).  
No manual setup is needed.

### For Developers

1. Add **Conductor** to your dependencies in `thunderstore.toml`:

   ```toml
   [package.dependencies]
   Conductor-Conductor = "0.1.9"
   ```

2. If testing locally, download the latest release and extract it into your `BepInEx/plugins` folder.

3. *(Coming soon)* If you’re using Conductor’s utility functions as part of your build:

   * Add a package reference to your `.csproj`:

     ```xml
     <PackageReference Include="Conductor" Version="0.1.9" />
     ```
   * Install the NuGet package once published.

---

## Usage

Check out the [**Conductor Wiki**](https://github.com/Monster-Train-2-Modding-Group/Conductor/wiki) for setup guides, API references, and examples.

>  **Important:**
> Do **not** copy any code or JSON directly from this repository.
> Many assets depend on Conductor internals and will not function standalone.
> Instead, **reference** Conductor’s effects, statuses, and utilities within your own project.

---

## Features at a Glance

* Ready-to-use custom **effects**, **statuses**, and **keywords**
* Quality-of-life APIs to simplify **mod logic**
* Extensible systems built for **integration with Trainworks-Reloaded**
* Future NuGet support for **direct library usage**

---

## Attribution

Icons used in this project are credited as follows:

* **Vengeance icon:** [Knight icons created by Freepik – Flaticon](https://www.flaticon.com/free-icons/knight)
* **Encounter icon:** [Find icons created by Freepik – Flaticon](https://www.flaticon.com/free-icons/find)
* **Junk icon:** [Recycle bin icons created by cah nggunung – Flaticon](https://www.flaticon.com/free-icons/recycle-bin)
* **Intangible icon:** [Ghost icons created by Aldo Cervantes – Flaticon](https://www.flaticon.com/free-icons/ghost)
* **Smirk icon:** [*Smirk* | Megami Tensei Wiki | Fandom](https://megamitensei.fandom.com/wiki/Smirk)

---

## Contributing

Contributions and pull requests are always welcome!
If you’d like to help expand Conductor’s features or documentation:

* Open an issue with feature requests or bug reports
* Submit a pull request
* Join the discussion on the MT2 Discord [#mt2-modding](https://discord.com/channels/336546996779483136/1377778943674810368) channel.

---

