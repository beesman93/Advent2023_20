using Advent2023_20;
using System.Runtime.Serialization;

List<string> lines = new();
using (StreamReader reader = new(args[0]))
    while (!reader.EndOfStream)
        lines.Add(reader.ReadLine() ?? "");

Dictionary<string, Module> modules = new();

/** INIT MODULES **/
foreach (var line in lines)
{
    var ls = line.Split(" -> ");
    string moduleName = ls[0];
    var destinations = ls[1].Split(", ");
    switch (moduleName[0])
    {
        case '%':
            modules.Add(moduleName[1..], new FlipFlop(moduleName[1..], destinations));
            break;
        case '&':
            modules.Add(moduleName[1..], new Conjuntion(moduleName[1..], destinations));
            break;
        default:
            modules.Add(moduleName, new Broadcast(moduleName, destinations));
            break;
    }
}
Dictionary<string, Conjuntion> conjunctions = [];
foreach(var conjunction in modules.Values.OfType<Conjuntion>())
    conjunctions.Add(conjunction.ID, conjunction);

foreach (var module in modules.Values)
    foreach (var destinationId in module.Destinations)
        if (conjunctions.TryGetValue(destinationId, out var conjuntion))
            conjuntion.addMemoryCell(module.ID);

part1();
resetModules();
part2();
void part1()
{
    (uint? lows, uint? highs) count = (0, 0);
    for (int i = 0; i < 1000; i++)
        pushButton(ref count, null, i, null);
    Console.WriteLine($"part1:\t\t{count.lows * count.highs}");
}
void part2()
{
    /*
     *  The final machine responsible for moving the sand down to Island Island has a module attached named rx.
     *  The machine turns on when a single low pulse is sent to rx.
     *  Reset all modules to their default states. Waiting for all pulses to be fully handled after each button press,
     *  what is the fewest number of button presses required to deliver a single low pulse to the module named rx?
     *
     *  Assuming from my input that rx is always targeted by a single Conjuntion
     *  Figure out period(s) of button press counts each memory cell in the conjunction memory
     *  and use chinease reminder theorem probably?
     *  If all periods are simple just return LCM
     *  
     *  Seems all periods are simple the endNode receives just other conjuctions that emit on a period
     *  everything leading to them is a flip-flop counter it seems? they reset before button press ends but all would
     *  emit high before resetting on the LCM where they meet - so just check for period for each
     *  could just terminate when receiving first flip from all but had period checker ready
     *  confirmed period is stored in the emitSourceIterFlip dict as a negative value, so check we have values in dict
     *  and all are negative and LCM the vals
     *  
     *  //modules.Add("rx", new Broadcast("rx", [])); -- wont emit to here in reality, no point adding
     */

    Conjuntion? endNode = findConjunctionTargetting("rx");
    Dictionary<string, int>? emitSourceIterFlip = [];
    for (int i = 1; i < int.MaxValue; i++)
    {
        (uint?, uint?) discardCounts = (null, null);
        pushButton(ref discardCounts, endNode, i, emitSourceIterFlip);
        bool foundSolution = false;
        if (emitSourceIterFlip.Count == endNode.Memory.Count)
        {
            foundSolution = true;
            foreach (var period in emitSourceIterFlip.Values)
                if (period > 0)
                {
                    foundSolution = false; break;
                }
        }
        if (foundSolution)
        {
            List<ulong> periods = [];
            foreach (int val in emitSourceIterFlip.Values)
                periods.Add(Convert.ToUInt64(Math.Abs(val)));
            Console.WriteLine($"part2:\t\t{periods.Aggregate(LCM)}");
            break;
        }
    }

}

Conjuntion findConjunctionTargetting(string destinationId)
{
    foreach (var conjunction in conjunctions.Values)
        if (conjunction.Destinations.Contains(destinationId))
            return conjunction;
    throw new Exception($"No conjunction targets{destinationId}");
}
ulong GCD(ulong a, ulong b) => b == 0 ? a : GCD(b, a % b);
ulong LCM(ulong a, ulong b) => a * b / GCD(a, b);

void resetModules()
{
    foreach (var module in modules.Values)
        module.reset();
}
void pushButton(ref (uint? lows, uint? highs)count, Conjuntion? endNode, int iter, Dictionary<string,int>?emitSourceFlipped)
{
    Queue<(string source, bool signal, string destination)> pulses = new();
    pulses.Enqueue(("button module", false, "broadcaster"));
    while (pulses.TryDequeue(out var pulse))
    {
        if (pulse.signal)
            count.highs++;
        else
            count.lows++;
        if (modules.TryGetValue(pulse.destination, out Module? module))
        {
            module.receive((pulse.source, pulse.signal));
            if (module == endNode && pulse.signal)
            {
                if (emitSourceFlipped.ContainsKey(pulse.source))
                {
                    if (iter % emitSourceFlipped[pulse.source] == 0)//found a simple period
                        emitSourceFlipped[pulse.source] *= -1;
                }
                else //first flip
                    emitSourceFlipped[pulse.source] = iter;
            }
            var (source, signal, destinations) = module.emit();
            foreach (var destination in destinations)
            {
                pulses.Enqueue((source, signal, destination));
            }
        }
    }
}