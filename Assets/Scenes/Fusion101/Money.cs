using UnityEngine;
using Fusion;

public class Money : NetworkBehaviour
{
    [Networked] public int amount {get;set;}

    public static void Transfer(NetworkObject from, NetworkObject to, bool all=true) {
        var fromMoney = from.GetComponent<Money>();
        var toMoney = to.GetComponent<Money>();
        if(fromMoney && toMoney && fromMoney.amount > 0) {
            toMoney.amount += fromMoney.amount;
            Debug.Log($"{from.name} gave {fromMoney.amount} to {to.name} player now has {toMoney.amount}");
            fromMoney.amount = 0;
        }
    }
    public static void Transfer(NetworkObject from, NetworkObject to, int count) {
        var fromMoney = from.GetComponent<Money>();
        var toMoney = to.GetComponent<Money>();
        if(fromMoney && toMoney) {
            count = Mathf.Min(count,fromMoney.amount);
            toMoney.amount += count;
            fromMoney.amount -= count;
            Debug.Log($"{from.name} gave {count} to {to.name} player now has {toMoney.amount}");
        }
    }

}