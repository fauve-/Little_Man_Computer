using System;
using System.Collections.Generic;
using System.Collections.Specialized;


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
  private int dataPointer;
  private int inbox;
  private int outbox;
  private int accumulator;
  public Interpreter(string program)
  {
    prog = program.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
    mailboxes = new Dictionary<int,int>();
    instructionPointer = 0;
    dataPointer = 0;
    inbox = 0;
    outbox = 0;
    accumulator = 0;
  }
  public void Interpret()
  {
    while(instructionPointer < prog.Length)
      {
        System.Tuple<int,int> parsed = parseInstruction();
        dispatch(parsed.Item1,parsed.Item2);
        instructionPointer++;
      }
  }

  
  private System.Tuple<int,int> parseInstruction()
  {
    string ins = prog[instructionPointer].ToString();
    string arg = ins[1].ToString();
    arg+=ins[2];
    return new System.Tuple<int,int>(Convert.ToInt32(ins[0]), 
                                     Convert.ToInt32(arg));
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