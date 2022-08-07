using Discord;
using Discord.WebSocket;
using System.Text.RegularExpressions;

public class UserWithMessages
{
    public ulong UserId;
    public ulong MessageId;

    public UserWithMessages(ulong userID, ulong messageID)
    {
        UserId = userID;
        MessageId = messageID;
    }
}
internal class Program1
{
    public static DiscordSocketClient _client;
    public static List<ulong> assignRoleIDs = new List<ulong>();
    public static string databasePath;
    public static string BotToken;
    public static ulong assignRoleChatID;
    public static ulong guildID;
    public static ulong configChatID;

    public static List<UserWithMessages> users = new List<UserWithMessages>();

    public static async Task CheckConfigChannel()
    {

        try
        {
            Console.WriteLine(guildID);
            Console.WriteLine(configChatID);
            var msgs = await _client.GetGuild(guildID).GetTextChannel(configChatID).GetMessagesAsync(1).ToListAsync();
            if (msgs.Count > 0)
            {
                if (msgs.ToList()[0].ToList()[0].MentionedRoleIds.Count > 0)
                {
                    assignRoleIDs.Clear();

                    assignRoleIDs.AddRange(msgs.ToList()[0].ToList()[0].MentionedRoleIds.ToList());
                }


                if (msgs.ToList()[0].ToList()[0].MentionedChannelIds.Count > 0)
                {
                    assignRoleChatID = msgs.ToList()[0].ToList()[0].MentionedChannelIds.ToList()[0];
                }
            }

        }catch(Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
      
    }
    public static void Main()
    {


        databasePath = Environment.CurrentDirectory;


        string ConfigPath = databasePath;
        ConfigPath += "/Config.txt";
        databasePath += "/Database";
        configChatID = 0;
        guildID =0;
        assignRoleChatID = 0;

        var config = File.ReadAllText(ConfigPath).Replace("\r", "\n").Split('\n');
       
        bool isBottokenset = false;
        bool isAssignChatIDSet = false;
        bool isRoleIDsSet = false;
        foreach (var item in config)
        {
          if(item.IndexOf("BotToken:") != -1)
            {
                if(!isBottokenset)
                {
                    var token = item;
                    token = token.Substring(token.IndexOf("BotToken:") + "BotToken:".Length).Trim();
                    BotToken = token;
                    isBottokenset = true;

                }
             
            }
          else if(item.IndexOf("AssignRoleChatID:") != -1)
            {
                if(!isAssignChatIDSet)
                {
                    var chatid = item;
                    assignRoleChatID = ulong.Parse(chatid.Substring(chatid.IndexOf("AssignRoleChatID:") + "AssignRoleChatID:".Length).Trim());
                    isAssignChatIDSet = true;
                    Console.WriteLine(assignRoleChatID);
                }
            
            }
            else if(item.IndexOf("AssignRoleIDs:") != -1)
            {
                if(!isRoleIDsSet)
                {
                    var roles = item;
                    roles = item.Substring(item.IndexOf("AssignRoleIDs:") + "AssignRoleIDs:".Length);

                    if (roles.Contains("#"))
                    {
                        foreach (var role in Regex.Split(roles, "#"))
                        {
                            assignRoleIDs.Add(ulong.Parse(role.Trim()));
                        }
                    }
                    else
                    {
                        assignRoleIDs.Add(ulong.Parse(roles.Trim()));
                    }

                    isRoleIDsSet = true;
                }
               
            }else if(item.IndexOf("ConfigChatID:") != -1)
            {
                configChatID = ulong.Parse(item.Substring(item.IndexOf("ConfigChatID:") + "ConfigChatID:".Length).Trim());
            }
            else if (item.IndexOf("GuildID:") != -1)
            {
                guildID = ulong.Parse(item.Substring(item.IndexOf("GuildID:") + "GuildID:".Length).Trim());
            }
        }

      

       
        
       

        var datas = File.ReadAllTextAsync(databasePath).GetAwaiter().GetResult();
      
        foreach (var m in Regex.Split(datas, "#"))
        {
            if (m != "")
            {
                if (!string.IsNullOrWhiteSpace(m))
                {
                    var usrid = ulong.Parse(m.Remove(m.IndexOf("$")).Trim());
                    var msgid = ulong.Parse(m.Substring(m.IndexOf("$")).Replace("$", "").Trim());

                    users.Add(new UserWithMessages(usrid, msgid));
                    
                }
            }
           
        }


       
        Console.WriteLine(databasePath);
        MainAsync().Wait();
    }
    public static async Task MainAsync()
    {



      
        _client = new DiscordSocketClient(new DiscordSocketConfig
        {
            GatewayIntents =
            GatewayIntents.Guilds |
            GatewayIntents.GuildMembers |
            GatewayIntents.GuildMessageReactions |
            GatewayIntents.GuildMessages |
            GatewayIntents.GuildVoiceStates | GatewayIntents.All

        });




        _client.Log += Log;

        await _client.LoginAsync(TokenType.Bot, BotToken);
        await _client.StartAsync();


        _client.MessageReceived += MessageHandler;

        _client.Ready += _client_Ready;
        await Task.Delay(-1);




    }

    private static Task _client_Ready()
    {

        Console.WriteLine("Logined as " + _client.CurrentUser.Username + "#" + _client.CurrentUser.Discriminator);
        Console.WriteLine("Made by LindaMosep");
        CheckConfigChannel().Wait();
        return Task.CompletedTask;
    }

    private static Task MessageHandler(SocketMessage e)
    {
        MessageRecieved(e).Wait();
        return Task.CompletedTask;
    }

    public static async Task MessageRecieved(SocketMessage e)
    {
        var usr = e.Author as SocketGuildUser;
        if (usr != null)
        {
           
            if (e.Channel.Id.ToString().StartsWith(assignRoleChatID.ToString()))
            {
             

                foreach(var assignRoleID in assignRoleIDs)
                {
                    if (usr.Roles.ToList().Find(m => m.Id == assignRoleID) == null)
                    {
                        if (usr.Guild.Roles.ToList().Find(m => m.Id == assignRoleID) != null)
                        {
                            await usr.AddRoleAsync(usr.Guild.Roles.ToList().Find(m => m.Id == assignRoleID));
                        }
                    }
                }
                



                var chnl = e.Channel as SocketTextChannel;

                if (users.Find(m => m.UserId.ToString().Contains(e.Author.Id.ToString())) != null)
                {
                    try
                    {
                        await chnl.DeleteMessageAsync(users.Find(m => m.UserId == e.Author.Id).MessageId);
                    }catch(Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                    finally
                    {
                        File.WriteAllText(databasePath, File.ReadAllText(databasePath).Replace(users.Find(m => m.UserId == e.Author.Id).MessageId.ToString(), e.Id.ToString()));
                        users.Find(m => m.UserId == e.Author.Id).MessageId = e.Id;
                    }
                  

                  
                    

                }
                else
                {
                    users.Add(new UserWithMessages(e.Author.Id, e.Id));
                    var list = File.ReadAllLines(databasePath).ToList();
                    list.Add(e.Author.Id + " $ " + e.Id + " #");
                    File.WriteAllLines(databasePath, list);
                }




            }
            if (e.Channel.Id.ToString().StartsWith(configChatID.ToString()))
            {
                if(e.MentionedChannels.Count > 0)
                {
                    var chnl = e.Channel as SocketTextChannel;
                    try
                    {
                        if (chnl.Guild.GetTextChannel(e.MentionedChannels.ToList()[0].Id) != null)
                        {
                            assignRoleChatID = e.MentionedChannels.ToList()[0].Id;
                        }
                    }catch(Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                   
                        
                }

                if(e.MentionedRoles.Count > 0)
                {
                    assignRoleIDs.Clear();
                  
                    assignRoleIDs.AddRange((e as IMessage).MentionedRoleIds.ToList());

                }
            }
        }   



    }
    private static Task Log(LogMessage arg)
    {
        Console.WriteLine(arg.Message);
        return Task.CompletedTask;
    }
}
