using System.Threading.Channels;
using Microsoft.AspNetCore.SignalR;

public class MessageStreamer( ChannelReader<TWWWSSMessage> channelReader) : Hub {

    public ChannelReader<TWWWSSMessage> StreamData() {
        // This method returns the channel reader to the client.
        // The client can then read messages from this channel.
        return channelReader;
    }
}