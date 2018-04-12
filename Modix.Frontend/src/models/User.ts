export type UserRole = "Member" | "Staff" | "Invalid";

export default class User
{
    name: string = "";
    userId: string = "";
    avatarHash: string = "";
    userRole: UserRole = "Invalid";

    get avatarUrl(): string
    {
        return `https://cdn.discordapp.com/avatars/${this.userId}/${this.avatarHash}`;
    }

    deserializeFrom(input: any): User
    {
        var self: any = this;
        
        for (let prop in input)
        {
            self[prop] = input[prop];
        }

        return this;
    }
}