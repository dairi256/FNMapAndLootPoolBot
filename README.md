# Fortnite Battle Bus Bot  ![Static Badge](https://img.shields.io/badge/IN%20DEVELOPMENT-081461?style=for-the-badge) ![GitHub branch status](https://img.shields.io/github/checks-status/dairi256/FNBattleBusBot/master?style=for-the-badge)



A custom Dicord bot created using C# and Discord.Net that provides users with the related Fortnite commands.

## Features
* **Current Map Command(`/map`):** When you type in this command, it will post an image of the current map from Fortnite API.
* **Discord Slash Commands:** Uses modern features for an easy and clean user expeience without the hassle of remembering commands.

## Planned Features
* Notification system for map changes.

## Setup and Installation
1. **Download Files:** Please download by ZIP.
2. **Bot Token::** If you are running your own instance, you must obtain your own key from the Discord Developer Portal and paste it into `appsettings.json` along with your server ID.
3. **Run the Excecutable** Double-CLick the exe file (`FNMapAndLootPoolBot.exe`)
* **Disclaimer: Excecutable is not available as of yet, I am hoping to create it when I feel like the bot is ready.**

## Bot Commands

All commands are Slash Commands (`/`) and will appear as you type.

| Command | Description | Example |
| :--- | :--- | :--- |
| `/map` | Displays the current Fortnite Battle Royale map with Points of Interest (POIs). | `/map` |
| `/shop` | Displays the current Item Shop. | `/shop` |

**Please Note:** All information is currently retreived via Fortnite-API.com. You can check their API Documentation here: https://dash.fortnite-api.com/

## Developer Setup (C#)

This project uses **C#** and the **.NET 9.0**, however, I will also be considering changing to **.NET 10.0**.

1.  **Clone the Repository:**
    ```bash
    git clone [https://github.com/YourUsername/YourRepoName.git](https://github.com/YourUsername/YourRepoName.git)
    ```
2.  **Install Dependencies:** All dependencies (Discord.Net, etc.) are handled by NuGet when the solution is built.
3.  **Configure Secrets:** This project uses **User Secrets** for development security of my tokens.
    * Right-click the project in Visual Studio.
    * Select **Manage User Secrets**.
    * Add your `DiscordToken` and `TestGuildId`.

4.  **Run:** Launch the project in Visual Studio (F5).

### This project is licensed under the MIT License. Read `LICENSE` for more information.
