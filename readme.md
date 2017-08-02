<p align="center"><img src="https://imagr.eu/up/oGOcm_eJwFwdsNwyAMAMBdGACrNs8sU1GCSCQSI-yqH1V3793XvNcwmzlUp2wA-ymV125FeZXebGfuo5V5iq18QVEt9bjarQJI2VHCQC48yEXCDJgooHc5pJh9wOgJBnd-vj523t38_ggIIs0.png" /></p>

[![Build status](https://ci.appveyor.com/api/projects/status/fdt0b8ucbwsotphm/branch/master?svg=true)](https://ci.appveyor.com/project/Cisien/modix/branch/master)

# MODiX - A moderation and analysis bot for discord

MODiX is a moderation and analysis bot for discord. As for now, it just deals with moderation, but development should be driven towards analytics to reach our goal in the near future.

## Roadmap & Contributions

Issues are the center of MODiX´s development. You can see what features are being worked on in the issues and you can see which features are planned. You might also create issues to share problems/ideas. 

## REPL

Since we get a lot of "questions" regarding this, I think at that point, I should make some kind of statement about it.

Thanks to the cooperation of Cisien, we managed to get a REPL up and running. Aim was to keep it as open and flexible as possible while keeping the response & execution times as low as possible. We just had to restrict some namespaces and classes to minimize the risk of abuse and we were good. 

Here is how its done:
1. Modix receives !exec-Message
2. Modix parses message, sends a HTTP Post-Request to Cisiens API
3. Cisiens API filters some bad namespaces/types, then compiles and executes the code
4. Cisiens API returns a JSON string which contains the results and some metadata
5. Modix wraps the result into a structure using JsonConvert<>
6. Modix returns the results to discord

Why we do it that way:
- Complete seperation between evaluation and discord bot
- Sandboxed environment for script execution
- Eliminating the risk of some compilation/execution crashing Modix
- Azure function restarts itself automagically when it crashes

> Thats not how you should do it

It works fine so far.

> You shouldn´t run a public REPL

We had no serious abuse after running it for almost 14 days.

> Running it in the executing assembly would be way faster

And also result in REPL-Sessions that can potentially crash the bot.
