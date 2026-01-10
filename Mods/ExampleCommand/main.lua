function OnInit()
    Console.RegisterCommand(
        "hello",
        "Hello i'm a very friendly example command",
        "hello [name]",
        function(args)
            if #args > 0 then
                local name = args[1]
                Console.LogSuccess("Hello " .. name .. "!")
            else
                Console.LogSuccess("Hello world!")
            end
        end
    )
end