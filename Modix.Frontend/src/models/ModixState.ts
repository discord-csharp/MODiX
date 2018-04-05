import User from "@/models/User";
import GuildInfoResult from "@/models/GuildInfoResult";
import UserCodePaste from "@/models/UserCodePaste";
import { ModuleHelpData } from "@/models/ModuleHelpData";

export default interface ModixState
{
    user: User;
    guildInfo: Map<string, GuildInfoResult>;
    errors: string[];
    pastes: UserCodePaste[];
    currentPaste: UserCodePaste | null;
    commands: ModuleHelpData[];
}