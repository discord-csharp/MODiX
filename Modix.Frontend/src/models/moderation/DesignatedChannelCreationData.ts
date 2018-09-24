import { ChannelDesignation } from '@/models/moderation/ChannelDesignation';

export default interface DesignatedChannelCreationData
{
    channelId: string;
    channelDesignations: ChannelDesignation[];
}