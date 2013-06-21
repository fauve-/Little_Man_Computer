using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections;
using System.Text;
/*
  So, what's happening here?
  The little man computer is a teaching too used to illustrate the Von Neumann
  architecture to students.
  There are 100 mailboxes (here represented by the dictionary<int,int> state.
  these mailboxes hold both instructions and data.
  The instruction pointer starts at zero and moves onward until it hits a 000 
  (hlt) instruction and quits, or a branch, and execution carries on elsewhere.

  The instruction set is thus:

  1XX ADD -> Adds whatever value is in box XX to the accumulator. 
             The result is stored in the accumulator.
  2XX SUB -> Subtracts whatever value is in box XX from the accumulator.
             Again, the result is stored in the accumulator.
  3XX STA -> Stores the accumulator in box XX.
  
  5XX LDA -> Loads whatever is in box XX into the accumulator.

  6XX BRA -> Sends the instruction pointer to box XX unconditionally.

  7XX BRZ -> Sends the instruction pointer to box XX if the accumulator is zero.
  
  8XX BRP -> Sends the instruction pointer to box XX if the accumulator is 
             positive.
  901 INP -> Takes input from the user.

  902 OUT -> Outputs a value to the user
  


  SPECIAL MACROS ->>

          CORE_DUMP -> will dump a grid of all registers
          
          JOHNNY_M -> a mnemonic-ified core dump
          

  TODO: Refactor into something that isn't a sketch. 
        Develop something resembling a user interface.
        Build a proper AST, allow labels, mnemonics w/args.
           ^this might be goofy for a glorified turing machine.

        search soul, consider writing assembler for 6502.
        
        ALSO CHANGE EMACS DEFAULT GODAWFUL BRACING STYLE. K+R 4 LYFE
        
        
        

*/

public class Program
{
  public static void Main(string[] args)
  {
    if(args.Length == 0)
      {
        Console
          .WriteLine("Please include a filename with LMC code as an argument");
        return;
      }
    else
      {
        foreach(string file in args)
          {
            string progn = System.IO.File.ReadAllText(file);
            Interpreter inter = new Interpreter(progn);
            inter.Interpret();
            Console.WriteLine(String.Format("Finished program {0}",file));
          }
      }
  }
}




public class Interpreter
{
  private string[] prog;
  private Dictionary<int,int> mailboxes; //state
  private int instructionPointer;
  private int accumulator;
  public Interpreter(string program)
  {
    prog = program.Split(new char[] {   ' ', '\n'} , 
                         StringSplitOptions.RemoveEmptyEntries);                
    mailboxes = new Dictionary<int,int>();
    instructionPointer = 0;
    accumulator = 0;
    loadProgram();
  }


  //could probably be abstracted away into a parser class, or at least drop
  //drop the helper functions into a static class of some kind
  private void loadProgram()
  {
    foreach(var addr in Enumerable.Range(0,prog.Length))
      {
        if(prog[addr] == "CORE_DUMP")
          {
            mailboxes[addr] = 991;
            continue;
          }
        if(prog[addr] == "JOHNNY_M")
          {
            mailboxes[addr] = 992;
            continue;
          }

        mailboxes[addr] = Convert.ToInt32(prog[addr]);
      }
      //fills out rest of mailboxes
      foreach(var addr in Enumerable.Range(prog.Length + 1, 101))
      {
      mailboxes[addr] = 0;
      }
      
  }

  
  public void Interpret()
  {
    while(instructionPointer < prog.Length)
      {
        Tuple<int,int> parsed = parseInstruction();
        dispatch(parsed.Item1,parsed.Item2);
        instructionPointer++;
      }
  }

  
  private Tuple<int,int> parseInstruction()
  {
    int ins = mailboxes[instructionPointer] / 100;
    int arg = mailboxes[instructionPointer] % 100;
    return new Tuple<int,int>(ins, arg);
  }
  
  private void dispatch(int instruction, int arg)
  {
    switch (instruction)
      {
      case 1:
        accumulator+=mailboxes[arg]; //add
        break;
      case 2:
        accumulator-=mailboxes[arg]; //sub
        break;
      case 3:
        mailboxes[arg] = accumulator; //save
        break;
      case 5:
        accumulator = mailboxes[arg]; //load
        break;
      case 6:
        instructionPointer = arg; //always branch
        break;
      case 7:
        if(accumulator == 0) //branch if zero
          {
            instructionPointer = arg;
          }
        break;
      case 8:
        if(accumulator > 0)//branch if positive
          {
            instructionPointer = arg;
          }
        break;
      case 9:
        if(arg == 1)
          {
            Console.WriteLine("Inbox requires input");
            accumulator = Convert.ToInt32(Console.ReadLine());
          }
        else if(arg == 2)
          {
            Console.WriteLine(accumulator);
          }
        else if(arg == 91)
          {
            coreDump();
          }
        else if(arg == 92)
          {
            prettierCoreDump();
          }
        else
          {
            throw new Exception("Invalid argument to 900 instruction");
          }
        break;
      case 0:
        break;
      default:
        throw new Exception(
                            String.Format("invalid instruction {0}", 
                                          instruction));
      }
  }


  //these belong in the parsing unit.
  private void coreDump()
  {
    StringBuilder sb = new StringBuilder();
    sb.AppendLine("CORE DUMP INCOMING");
    foreach(var i in mailboxes.Keys)
      {
        sb.Append(mailboxes[i].ToString("D3"));
        sb.Append(" ");
        if(i % 10 == 0 && i != 0)
          {
            sb.AppendLine("");
          }
      }
    sb.AppendLine("");
    Console.WriteLine(sb.ToString());
  }

  //note: macro transformation has already taken place.
  private void prettierCoreDump()
  {
    Dictionary<int,string> tt = new Dictionary<int,string>();//translation table
    tt.Add(1,"ADD");
    tt.Add(2,"SUB");
    tt.Add(3,"STA");
    tt.Add(5,"LDA");
    tt.Add(6,"BRA");
    tt.Add(7,"BRZ");
    tt.Add(8,"BRP");
    tt.Add(9, "SPECIAL");
    tt.Add(0,"HALT");
    StringBuilder sb = new StringBuilder();
    sb.AppendLine("Note, there is no way to tell the difference between code and data in this architecture. All mailboxes will be interpreted as code.");
    sb.AppendLine("The best way to tell if something is a data value is if it registers as a halt with an argument that isn't zero.");
    foreach(var i in mailboxes.Keys)
        {
          var box =  mailboxes[i];
          var inst = box/ 100;
          var arg = box % 100;
          sb.Append(tt[inst]);
          sb.Append(" ");
          sb.Append(arg);
          sb.AppendLine();
        }
    Console.WriteLine(sb.ToString());
  }

}