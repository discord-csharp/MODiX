export interface ModuleHelpData
{
    name: string;
    summary: string;
    commands: CommandHelpData[];
}

export interface CommandHelpData
{
    alias: string;
    name: string;
    summary: string;
    parameters: ParameterHelpData[];
}

export interface ParameterHelpData
{
    name: string;
    summary: string;
    type: string;
    isOptional: boolean;
}