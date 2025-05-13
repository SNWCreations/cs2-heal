using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;

namespace cs2_heal;

public class Heal : BasePlugin
{
    public override string ModuleName => "heal";
    public override string ModuleVersion => "1.0.0";
    public override string ModuleAuthor => "SNWCreations";
    public override string ModuleDescription => "Health & Armor Value setters";

    public static CCSPlayerController? GetPlayerByName(string name)
    {
        return Utilities.GetPlayers().Find(it => !it.IsBot && it.PlayerName == name);
    }

    [ConsoleCommand("css_heal", "Heal yourself")]
    [RequiresPermissions("@css/cheats")]
    public void OnCmdHeal(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null || player.Team == CsTeam.None || player.Team == CsTeam.Spectator)
        {
            return;
        }

        var pawn = player.Pawn.Get()?.As<CCSPlayerPawn>();
        if (pawn != null)
        {
            pawn.Health = pawn.MaxHealth;
            pawn.ArmorValue = 100;
            Utilities.SetStateChanged(pawn, "CCSPlayerPawn", "m_ArmorValue");
            Utilities.SetStateChanged(pawn, "CBaseEntity", "m_iHealth");
        }
    }

    [ConsoleCommand("css_kill", "Kill someone, or yourself")]
    [RequiresPermissions("@css/cheats")]
    [CommandHelper(usage: "[target]", whoCanExecute: CommandUsage.CLIENT_ONLY)]
    public void OnCmdKill(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null && info.ArgCount == 1)
        {
            info.ReplyToCommand("Must specify a player as target");
            return;
        }

        CCSPlayerController targetController;
        if (info.ArgCount == 1)
        {
            targetController = player!;
        }
        else
        {
            var targetName = info.GetArg(1);
            var found = GetPlayerByName(targetName);
            if (found != null)
            {
                targetController = found;
            }
            else
            {
                info.ReplyToCommand("Player not found");
                return;
            }
        }

        var pawn = targetController.Pawn.Get()?.As<CCSPlayerPawn>();
        if (pawn != null)
        {
            pawn.CommitSuicide(false, true);
            info.ReplyToCommand("Operation successful");
        }
        else
        {
            info.ReplyToCommand("Target does not have a pawn bound to it");
        }
    }

    [ConsoleCommand("css_hurt", "Hurt someone, or yourself")]
    [RequiresPermissions("@css/cheats")]
    [CommandHelper(minArgs: 1, usage: "<amount> [target]", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
    public void OnCmdHurt(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null && info.ArgCount == 1)
        {
            info.ReplyToCommand("Must specify a player as target through the second argument");
            return;
        }

        CCSPlayerController target;
        if (info.ArgCount == 3)
        {
            var targetName = info.ArgByIndex(2);
            var search = GetPlayerByName(targetName);
            if (search != null)
            {
                target = search;
            }
            else
            {
                info.ReplyToCommand("Player not found");
                return;
            }
        }
        else
        {
            target = player!;
        }

        if (int.TryParse(info.ArgByIndex(1), out var amount))
        {
            if (amount > 0)
            {
                var pawn = target.Pawn.Get()?.As<CCSPlayerPawn>();
                if (pawn != null)
                {
                    var updated = pawn.Health - amount;
                    if (updated > 0)
                    {
                        pawn.Health = updated;
                        Utilities.SetStateChanged(pawn, "CBaseEntity", "m_iHealth");
                    }
                    else
                    {
                        pawn.CommitSuicide(false, true);
                    }
                    info.ReplyToCommand("Operation successful");
                }
                else
                {
                    info.ReplyToCommand("Player does not have a pawn bound to it");
                }
            }
            else
            {
                info.ReplyToCommand("Amount should be positive");
            }
        }
        else
        {
            info.ReplyToCommand("Amount should be an integer");
        }
    }

    [ConsoleCommand("css_setarmor", "Set durability value of someone's armor, or your own")]
    [RequiresPermissions("@css/cheats")]
    [CommandHelper(minArgs: 1, usage: "<amount> [damage]", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
    public void OnCmdSetArmor(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null && info.ArgCount == 1)
        {
            info.ReplyToCommand("Must specify a player as target through the second argument");
            return;
        }

        CCSPlayerController target;
        if (info.ArgCount == 3)
        {
            var targetName = info.ArgByIndex(2);
            var search = GetPlayerByName(targetName);
            if (search != null)
            {
                target = search;
            }
            else
            {
                info.ReplyToCommand("Player not found");
                return;
            }
        }
        else
        {
            target = player!;
        }

        if (int.TryParse(info.ArgByIndex(1), out var amount))
        {
            var pawn = target.Pawn.Get()?.As<CCSPlayerPawn>();
            if (pawn != null)
            {
                pawn.ArmorValue = Math.Min(100, Math.Max(0, amount));
                Utilities.SetStateChanged(pawn, "CCSPlayerPawn", "m_ArmorValue");
                info.ReplyToCommand("Operation successful");
            }
            else
            {
                info.ReplyToCommand("Player does not have a pawn bound to it");
            }
        }
        else
        {
            info.ReplyToCommand("Amount should be an integer");
        }
    }
}