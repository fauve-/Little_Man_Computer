using System;
using System.Collections.Generic;

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
    string ins = prog[instructionPointer];
    string arg = ins[1].ToString();
    arg+=ins[2];
    return new Tuple<int,int>(Int32.Parse(ins[0].ToString()), 
                              Int32.Parse(arg));
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
}