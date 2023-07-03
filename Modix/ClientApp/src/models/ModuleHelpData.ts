export interface ModuleHelpData
{
    name: string;
    summary: string;
    commands: CommandHelpData[];
}

export interface CommandHelpData
{
    aliases: string[];
    name: string;
    summary: string;
    parameters: ParameterHelpData[];
    isSlashCommand: boolean;
}

export interface ParameterHelpData
{
    name: string;
    summary: string;
    type: string;
    isOptional: boolean;
    options: string[];
}