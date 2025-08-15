using AdventureCore;
using AdventureExtras;
using System.Linq;

public class MultiArenaPlayerCharacter : ArenaPlayer
{
    public MultiArenaPlayer Networker;

    public override bool PreDamageReceive(IDamageSender sender, IDamageReceiver receiver)
    {
        if (Networker.CheckReceiveDamage(sender.Damages.FirstOrDefault()?.Kind))
            return base.PreDamageReceive(sender, receiver);
        else
            return false;
    }

    public override bool PreDamageSend(IDamageSender sender, IDamageReceiver receiver)
    {
        if (Networker.CheckSendDamage(receiver, sender.Damages.FirstOrDefault()?.Kind))
            return base.PreDamageSend(sender, receiver);
        else
            return false;
    }

    public override void OnDamageReceive(DamageEvent e)
    {
        base.OnDamageReceive(e);

        Networker.ReceiveDamage(e);
    }

    public override void OnDamageSend(DamageEvent e)
    {
        base.OnDamageSend(e);

        Networker.SendDamage(e);
    }
}