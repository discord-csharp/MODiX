import { ChannelDesignation } from '@/models/moderation/ChannelDesignation';

export default interface DesignatedChannelMapping
{
    id: string;
    channelId: string;
    channelDesignation: ChannelDesignation;
    name: string;
}