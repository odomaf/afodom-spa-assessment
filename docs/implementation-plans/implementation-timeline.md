# Implementation Timeline

This timeline supports the phase-gate implementation plan with bounded work sessions and planned recovery time.

All completion times use local offset format: `YYYY-MM-DD HH:MM (UTC±HH:MM)`.

## Schedule Constraints

- Work window: 10:00 AM to 8:00 PM.
- Work in defined chunks of 2-3 hours.
- Take 1-2 hour breaks between chunks.
- Preferred submission target: Thursday by 8:00 PM.
- Hard submission deadline: Thursday by 11:59 PM.

## Tuesday (Completed)

- Resolve scope documentation structure (keep Gate 0 summary + decisions in questions-and-assumptions).
- Reorganize planning docs into `docs/implementation-plans/`.
- Complete Gate 1 scaffolding (`SubawardReader.slnx`, app project, test project, package setup).
- Verify Gate 1 with `dotnet restore` and `dotnet build`.
- Add repository hygiene (`.gitignore`) and record Gate 1 completion markers.
- Update documentation for `.sln`/`.slnx` behavior and SDK troubleshooting notes.

## Wednesday (Execution)

- 10:00-12:30: Gate 2 (Core Design)
- 2:00-4:30: Gate 3 (Parsing Implementation, part 1)
- 6:00-8:00: Gate 3 (Parsing Implementation, part 2 + smoke validation)

## Thursday (Finish and Submit)

- 10:00-12:30: Gate 4 (Aggregation and Ordering)
- 2:00-4:30: Gate 5 (Console UX and Error Handling)
- 6:00-8:00: Gate 6 + Gate 7 + Gate 8 closeout and submit

## Risk Buffer

- Reserve 4:30-6:00 PM Thursday as contingency time.

## Execution Rules

- Start each chunk with one clear gate outcome.
- End each chunk with a checkpoint commit when feasible.
- Stop work at 8:00 PM and continue in the next scheduled chunk.
