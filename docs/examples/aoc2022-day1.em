import Standard.IO;

function Day01 begin
    function Part1(Input_File: String): Result[(), error] begin
        let File = try IO.Open(Input_File);
        defer File.Close();

        let Parsed_Lines = File.Lines().Map(function (Line) begin
            return if Line == "" then None
                else Some(try Int.Parse(Line))
        end);

        let Groups = Parsed_Lines.Group_At(None);

        let Result = Groups.Map(function (Group) Group.Sum())
            .Max();
        
        Print_Line($"Part 1 {Input_File} Result: {Result}");
    end;

    function Part2 begin
    end;

    Part1("test");
    Part1("input");
    Part2("test");
    Part2("input");
end Day01;