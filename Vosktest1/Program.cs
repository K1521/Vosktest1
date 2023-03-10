// See https://aka.ms/new-console-template for more information
using NAudio.Wave;
using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;
using Vosk;
using Vosktest1;
using static System.Runtime.InteropServices.JavaScript.JSType;



Console.WriteLine("Hello, World!");


// Output speakers

var grammar=JsonSerializer.Serialize(new List<string> { 
    "one two three",
    "six seven eight",
    "[unk]","[unk]","[unk]","[unk]","[unk]","[unk]",
    "felice"
});

var grammarlist = new List<string> {
"felice open interface",
"felice move up",
"felice move down",
"felice lean forward",
"felice lean backward",
"felice stop",
"felice set position",
"[unk]",
"felice shutdown"};

grammarlist.AddRange(NumberConverter.other.Concat(NumberConverter.tens).Concat(NumberConverter.ones).Distinct<string>());

string[] units = { "Zero", "One", "Two", "Three",
"Four", "Five", "Six", "Seven", "Eight", "Nine", "Ten", "Eleven",
"Twelve", "Thirteen", "Fourteen", "Fifteen", "Sixteen",
"Seventeen", "Eighteen", "Nineteen" };
string[] tens = { "", "", "Twenty", "Thirty", "Forty",
"Fifty", "Sixty", "Seventy", "Eighty", "Ninety" };
string Convert(Int64 i)
{
    if (i < 20)
    {
        return units[i];
    }
    if (i < 100)
    {
        return tens[i / 10] + ((i % 10 > 0) ? " " + Convert(i % 10) : "");
    }
    if (i < 1000)
    {
        return units[i / 100] + " Hundred"
        //+ ((i % 100 > 0) ? " And " + Convert(i % 100) : "");
        +((i % 100 > 0) ? " " + Convert(i % 100) : "");
    }
    if (i < 100000)
    {
        return Convert(i / 1000) + " Thousand"
    + ((i % 1000 > 0) ? " " + Convert(i % 1000) : "");
    }
    if (i < 10000000)
    {
        return Convert(i / 100000) + " Lakh "
        + ((i % 100000 > 0) ? " " + Convert(i % 100000) : "");
    }
    if (i < 1000000000)
    {
        return Convert(i / 10000000) + " Crore "
        + ((i % 10000000 > 0) ? " " + Convert(i % 10000000) : "");
    }
    return Convert(i / 1000000000) + " Arab "
    + ((i % 1000000000 > 0) ? " " + Convert(i % 1000000000) : "");
}

Random rnd = new Random();

int k = 1;
while (k < 1000000000) { 
for (int j = 0; j < 1000; j++)
    {
        int i= rnd.Next(1, k);
        if (i < 100000 && Convert(i).ToLower()!= NumberConverter.NumberToWord(i)) { 
            Console.WriteLine(i+ ": " + Convert(i).ToLower());
            Console.WriteLine(i + ": " + NumberConverter.NumberToWord(i));
        }
        if (NumberConverter.WordToNumber(NumberConverter.NumberToWord(i)) != i) {
            Console.WriteLine(i + ": " + NumberConverter.WordToNumber(NumberConverter.NumberToWord(i)));
            Console.WriteLine(i + ": " + NumberConverter.NumberToWord(i));
            Console.WriteLine();
        }
        int num;
        if(NumberConverter.TryWordToNumber(NumberConverter.NumberToWord(i),out num))
        {
            if(num!=i)
                Console.WriteLine((int)num+" "+i);
        }else
            Console.WriteLine(i + ": " + NumberConverter.NumberToWord(i)+" "+num);

        
        Match m = Regex.Match(NumberConverter.NumberToWord(i),"^"+ NumberConverter.renum+"$", RegexOptions.IgnoreCase);
        if (!m.Success)
            //Console.WriteLine("Found '{0}' at position {1}.", m.Value, m.Index);
        {
            Console.WriteLine(i + ": " + NumberConverter.NumberToWord(i));
            Console.WriteLine(":(");
            Console.WriteLine();
        }

        

    }
    k *= 10;
}

Regex re =new Regex("^" + NumberConverter.renum + "$");

for (int i = 0; i < 10000000; i++) {
    Match m = re.Match(NumberConverter.NumberToWord(i));
    if (!m.Success)
    //Console.WriteLine("Found '{0}' at position {1}.", m.Value, m.Index);
    {
        Console.WriteLine(i + ": " + NumberConverter.NumberToWord(i));
        Console.WriteLine(":(");
        Console.WriteLine();
    }
    if (i % 100000 == 0) Console.WriteLine(i);
}

for (int i = 0; i <= 10000; i++)
{
    var n = NumberConverter.NumberToWord(i).ToLower();
    //Console.WriteLine($"[{n}]");
    grammarlist.Add($"felice set linear {n}");
    grammarlist.Add($"felice set inclination {n}");
}
for (int i = 0; i < 100; i++)
{
    grammarlist.Add("[unk]");
}


/*
{
    "partial" : "felice set up"
}
{
    "alternatives" : [{
        "confidence" : 122.261612,
      "text" : "felice set up"
      }]
}

*/
//actual program
Console.WriteLine(System.AppDomain.CurrentDomain.BaseDirectory);
string modelpath= System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory,"..", "..",  "..", "voskmodel", "vosk-model-small-en-us-0.15");
Model model = new Model(modelpath);

bool running=true;
object runningpulse = new();

VoskRecognizer rec = new VoskRecognizer(model, 16000.0f, JsonSerializer.Serialize(grammarlist));
//rec.SetMaxAlternatives(3);
//rec.SetSpkModel(spkModel);

WaveFormat format = new WaveFormat(16000, 16, 1);
WaveInEvent waveIn=new WaveInEvent
{
    
    BufferMilliseconds = 50,
    DeviceNumber = 0, 
    WaveFormat = format
};

byte[] buff = new byte[4096];
string lastpartial = null;
void sourceStream_DataAvailable(object sender, WaveInEventArgs e)
{
    //Buffer.BlockCopy(e.Buffer, 0, buff,0,e.Buffer.Length);
    //if (rec.AcceptWaveform(buff, e.Buffer.Length))
    if (rec.AcceptWaveform(e.Buffer, e.Buffer.Length))
    {

        string res = rec.Result();
        var vtext = JsonSerializer.Deserialize<vosktext>(res);
        if (vtext.text != "") { 
            Console.WriteLine(res);
            if(NumberConverter.TryWordToNumber(vtext.text, out int num)) { 
                Console.WriteLine(num);
                Console.WriteLine(NumberConverter.NumberToWord(num));
            }
            lastpartial = null;

            if (vtext.text == "felice shutdown")
            {
                running = false;
                lock (runningpulse) Monitor.Pulse(runningpulse);
                Console.WriteLine("Stopping------------------");
            }
        }
    }
    else
    {
        string partial = rec.PartialResult();
        if (partial != lastpartial)
        {
            Console.WriteLine(partial);
            lastpartial = partial;
        }
        

    }


}

waveIn.DataAvailable += new EventHandler<WaveInEventArgs>(sourceStream_DataAvailable);
//waveIn.RecordingStopped += new EventHandler<StoppedEventArgs>(WaveIn_RecordingStopped);


waveIn.StartRecording();
while (running)lock(runningpulse)
    Monitor.Wait(runningpulse, 10000);
waveIn.StopRecording();
Thread.Sleep(10);
Console.WriteLine(rec.FinalResult());


