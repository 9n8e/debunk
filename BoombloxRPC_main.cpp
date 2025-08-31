#include <iostream>
#include <thread>
#include <string>
#include <chrono>
#include "discord_rpc.h"

void initDiscord() {
    DiscordEventHandlers handlers;
    memset(&handlers, 0, sizeof(handlers));

    Discord_Initialize("1354907878724730910", &handlers, 1, NULL);
}

void UpdatePresence(const char* state)
{
    DiscordRichPresence discordPresence;
    memset(&discordPresence, 0, sizeof(discordPresence));

    discordPresence.state = state;
    discordPresence.details = nullptr;
    discordPresence.startTimestamp = std::time(0);
    discordPresence.largeImageKey = "bbxb";
    discordPresence.largeImageText = "Boomblox";
    discordPresence.partySize = 1;
    discordPresence.partyMax = 1;

    Discord_UpdatePresence(&discordPresence);
}

int main(int argc, char* argv[]) {
    bool result = false;
    int value;
    try {
        value = std::stoi(argv[1]);
        result = true;
    }
    catch (const std::exception& e) {
        std::cout << e.what();
        return 0;
    }

    std::cout << value;

    const char* state = nullptr;
    if (result) {
        initDiscord();
        switch (value) {
        case 1:
            state = "Playing Online";
            break;
        case 2:
            state = "Playing Solo";
            break;
        case 3:
            state = "Editing";
            break;
        default:
            state = "";
            break;
        }
    }

    UpdatePresence(state);

    while (true) {
        Discord_RunCallbacks();
        std::this_thread::sleep_for(std::chrono::seconds(2));
    }

    Discord_Shutdown();
    return 0;
}
