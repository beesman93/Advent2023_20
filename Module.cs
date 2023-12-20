using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Advent2023_20
{
    abstract class Module
    {
        public readonly string ID;
        protected bool signal;
        public readonly string[] Destinations;
        public Module(string id, string[] destinations) => (ID, Destinations) = (id, destinations);
        public virtual (string source, bool signal, string[] destinations) emit()
        => (ID, signal, Destinations);
        public abstract void receive((string id, bool signal) pulse);
        public virtual void reset() => signal = false;

    }
    /*  Flip-flop modules (prefix %)
     *  are either on or off; they are initially off.
     *  If a flip-flop module receives a high pulse, it is ignored and nothing happens.
     *  However, if a flip-flop module receives a low pulse, it flips between on and off.
     *  If it was off, it turns on and sends a high pulse.
     *  If it was on, it turns off and sends a low pulse.*/
    class FlipFlop(string id, string[] destinations) : Module(id, destinations)
    {
        bool emits = false;
        public override void receive((string id, bool signal) pulse)
        {
            emits = !pulse.signal;
            if (!pulse.signal)
                signal = !signal;
        }
        public override (string source, bool signal, string[] destinations) emit()
        {
            if (emits) return base.emit();
            return (ID, false, []);
        }
        public override void reset()
        {
            emits = false;
            base.reset();
        }
    }
    /*  Conjunction modules (prefix &)
     *  remember the type of the most recent pulse received from each of their connected input modules;
     *  they initially default to remembering a low pulse for each input. When a pulse is received,
     *  the conjunction module first updates its memory for that input.
     *  Then, if it remembers high pulses for all inputs, it sends a low pulse;
     *  otherwise, it sends a high pulse.
     */
    class Conjuntion(string id, string[] destinations) : Module(id, destinations)
    {
        public readonly Dictionary<string, bool> Memory = [];
        public void addMemoryCell(string id) => Memory.Add(id, false);
        public override void receive((string id, bool signal) pulse)
        {
            Memory[pulse.id] = pulse.signal;
            signal = false;
            foreach (var mem in Memory.Values)
                if (!mem)
                {
                    signal = true;
                    break;
                }
        }
        public override void reset()
        {
            foreach (var memCellKey in Memory.Keys)
                Memory[memCellKey] = false;
            base.reset();
        }
    }
    /*  There is a single broadcast module (named broadcaster).
     *  When it receives a pulse, it sends the same pulse to all of its destination modules.
     */
    class Broadcast(string id, string[] destinations) : Module(id, destinations)
    {
        public override void receive((string id, bool signal) pulse)
        => signal = pulse.signal;
    }
}
