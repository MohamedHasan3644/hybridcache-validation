# Manual Test Execution Guide

This guide provides literal manual steps for the `CacheView` backed by HybridCache validation sample. It covers `TC-01` through `TC-12` and must be executed manually. Record the observed outcome; do not mark a case as passed until every listed verification succeeds.

## Read This First

### Start the required services

1. Open Windows PowerShell in `D:\HybridCacheValidation`.
2. If `docker` is not recognized in the VS Code terminal, run:

   ```powershell
   $env:Path += ";C:\Program Files\Docker\Docker\resources\bin"
   ```

3. Run the following command and wait for it to finish:

   ```powershell
   docker compose up -d redis proxy
   ```

4. Run:

   ```powershell
   docker compose ps
   ```

5. Confirm `hybridcachevalidation-redis-1` and `hybridcachevalidation-proxy-1` both show `Up`.
6. Confirm proxy port `8080` and Redis port `6379` are listed.

### Start the two application instances

1. Open a new VS Code terminal and enter:

   ```powershell
   Set-Location D:\HybridCacheValidation
   dotnet run --project .\HybridCacheValidation --urls http://localhost:5101 -- --Validation:InstanceName=Instance-A
   ```

2. Wait for `Now listening on: http://localhost:5101` and leave this terminal open. It is **Instance A**.
3. Open another new VS Code terminal and enter:

   ```powershell
   Set-Location D:\HybridCacheValidation
   dotnet run --project .\HybridCacheValidation --urls http://localhost:5102 -- --Validation:InstanceName=Instance-B
   ```

4. Wait for `Now listening on: http://localhost:5102` and leave this terminal open. It is **Instance B**.
5. Before each testcase, type a separator into a separate spare PowerShell terminal, or visually note the current bottom line of each application terminal. Do not send text to the terminal that is currently occupied by `dotnet run`.

### Prepare Chrome and evidence

1. Create `D:\HybridCacheValidation\Evidence` if it does not exist.
2. Close every existing Chrome Incognito window.
3. Open Chrome and press **Ctrl+Shift+N**.
4. Press **F12**, click **Network**, select **Preserve log**, then select **Disable cache**.
5. Click the clear icon in Network before each independent request flow.
6. Use a fresh `entry` value every time a case is rerun, for example `tc01-rerun-01`. Redis keeps entries for up to 10 minutes.
7. At every evidence checkpoint, press **Win+Shift+S**, capture the named area, click the Snipping Tool notification, select **Save as**, and save in `D:\HybridCacheValidation\Evidence` using the exact name shown.
8. For every `.txt` evidence file, follow **Create an evidence text file** below.

The phrase `CachedGuid initialized` means the child component actually rendered. Its absence on a cache-hit request is significant evidence.

### Create an evidence text file

Use this procedure at every checkpoint that names a `.txt` file.

1. Click the terminal containing the required output.
2. Click at the first line to retain, hold the left mouse button, drag to the last required line, then release the mouse button.
3. Press **Ctrl+C** to copy the selected terminal text. In Windows Terminal or VS Code's integrated terminal, copied text is automatically placed on the clipboard.
4. In VS Code, press **Ctrl+N** to open a new untitled text editor.
5. Press **Ctrl+V** to paste the terminal text.
6. For a manual observation file, type the requested values above the pasted text. For example:

   ```text
   Testcase: TC-06
   Initialized UTC: 2026-09-17T06:30:00.0000000+00:00
   Calculated expiry UTC: 2026-09-17T06:30:45.0000000+00:00
   Reload UTC: 2026-09-17T06:30:50.0000000+00:00

   Terminal output:
   ```

7. Press **Ctrl+Shift+S**. In the Save As dialog, enter `D:\HybridCacheValidation\Evidence\` followed by the exact filename from the checkpoint, such as `TC-01-02-A-terminal.txt`.
8. Click **Save**. If VS Code asks for a language mode, choose **Plain Text** or dismiss the prompt.
9. Confirm the filename is visible in the editor tab and ends in `.txt` rather than `.txt.txt`.
10. Reopen the file from the **Explorer** panel and verify it contains the expected terminal lines or observations before continuing.

For a terminal capture, include the request line, relevant cache/provider log lines, and initialization line when one is expected. When a testcase expects no initialization, include all output from immediately before the browser request through the final request line so the absence can be evaluated.

## TC-01 - Shared entry across instances

**Purpose:** Prove an entry warmed by A is served by B from the shared Redis-backed secondary cache.

1. Confirm Redis, A, and B are running.
2. In Chrome, click the address bar and enter `http://localhost:5101/?entry=tc01-01`.
3. Press **Enter** and wait for the page to finish loading.
4. Confirm **Responding instance** says `Instance-A`.
5. Under **Shared entry**, copy the Guid and Initialized UTC value.
6. Switch to A's terminal and locate the new `CachedGuid initialized` line with the same Guid.

**Evidence checkpoint - capture now:** Before opening B, capture the page address bar, `Instance-A`, cached variant, Guid, and Initialized UTC. Save as `TC-01-01-A-page.png`. Copy A terminal output beginning with the request through the matching initialization line to `TC-01-02-A-terminal.txt`.

7. Return to Chrome. Click the address bar and change only `5101` to `5102`; leave `?entry=tc01-01` unchanged.
8. Press **Enter** and wait for the page.
9. Confirm **Responding instance** says `Instance-B`.
10. Confirm B displays exactly the Guid and Initialized UTC copied from A.
11. Switch to B's terminal and inspect only the new lines written for this request.
12. Confirm B has no `CachedGuid initialized` line for this request.

**Evidence checkpoint - capture now:** Capture B's page with its full URL, `Instance-B`, cached variant, Guid, and Initialized UTC. Save as `TC-01-03-B-page.png`. Copy B terminal output for the B request to `TC-01-04-B-terminal.txt`; it must show the request context but no child-initialization line.

**Pass checklist:** A and B show the same Guid and timestamp; only A logs child initialization.

## TC-02 - Restart one instance

**Purpose:** Prove a restarted instance reuses an unexpired shared entry instead of rerendering it.

1. In Chrome, open `http://localhost:5101/?entry=tc02-01`.
2. Confirm `Instance-A`, then copy the Guid and Initialized UTC.
3. Confirm A terminal shows one matching `CachedGuid initialized` line.

**Evidence checkpoint - capture now:** Save the page as `TC-02-01-before-restart-page.png`. Copy A terminal lines for this initial request to `TC-02-02-before-restart-terminal.txt`.

4. Click A's terminal and press **Ctrl+C** once.
5. Wait until PowerShell returns to the prompt.
6. Run the original A command again:

   ```powershell
   dotnet run --project .\HybridCacheValidation --urls http://localhost:5101 -- --Validation:InstanceName=Instance-A
   ```

7. Wait for `Now listening on: http://localhost:5101`.
8. Return to the unchanged browser URL and press **Ctrl+R** before 10 minutes have passed since step 1.
9. Confirm the Guid and Initialized UTC are unchanged.
10. Inspect post-startup A logs. Confirm the reload did not produce a new `CachedGuid initialized` line.

**Evidence checkpoint - capture now:** Save the reloaded page as `TC-02-03-after-restart-page.png`. Copy A terminal output beginning at `Now listening on` through the reload request to `TC-02-04-after-restart-terminal.txt`.

**Pass checklist:** The post-restart page has the original Guid/timestamp and no new A child initialization.

## TC-03 - Restart both instances

**Purpose:** Count actual child renders as both application processes restart in turn.

1. Open `http://localhost:5101/?entry=tc03-01` in Chrome.
2. Copy the Guid and mark A's matching initialization log as render count `1`.
3. Stop A with **Ctrl+C**, start it again using the A command, and wait for port 5101 to listen.
4. Reload the unchanged A URL and confirm the original Guid/timestamp remains.
5. Stop B with **Ctrl+C**, start it again using the B command, and wait for port 5102 to listen.
6. Open `http://localhost:5102/?entry=tc03-01`.
7. Confirm B has the original Guid/timestamp.
8. Count every `CachedGuid initialized` line generated for this flow across both terminal captures.

**Evidence checkpoint - capture now:** Save A's final page as `TC-03-01-restarted-A-page.png` and B's final page as `TC-03-02-restarted-B-page.png`. Copy relevant output from both terminals to `TC-03-03-render-count-logs.txt`, adding `Observed initialization count: <number>` on the first line.

**Pass checklist:** Exactly one initialization occurred while the entry was unexpired: the first A request.

## TC-04 - Unsupported sliding expiration

**Purpose:** Capture the `ExpiresSliding` diagnostic from a separate page.

1. Confirm A is running and HybridCache is registered.
2. Open a new Chrome tab.
3. Enter `http://localhost:5101/sliding` and press **Enter**.
4. Wait for the error response or developer exception page.
5. Press **F12**, click **Console**, and copy any visible error text.
6. Switch to A terminal and copy the complete exception and stack trace.

**Evidence checkpoint - capture now:** Save the browser error page with the full URL as `TC-04-01-sliding-page.png`. Save browser Console output as `TC-04-02-browser-console.txt`; save A's full exception and stack trace as `TC-04-03-server-exception.txt`.

**Pass checklist:** The failure is `NotSupportedException` and its message names sliding expiration and points to absolute options such as `ExpiresAfter` or `ExpiresOn`.

## TC-12 - In-memory sliding-expiration control

**Purpose:** Demonstrate that the same `CacheView` succeeds with sliding expiration when no `HybridCache` service is registered.

1. Start the app without HybridCache: `dotnet run --project .\HybridCacheValidation --urls http://localhost:5103 -- --Validation:UseHybridCache=false --Validation:InstanceName=InMemory-Control`.
2. Open `http://localhost:5103/sliding` and copy the Guid.
3. Reload within 10 seconds and confirm the Guid is unchanged.
4. Wait more than 10 seconds without requesting the page, then reload and confirm the Guid changes.

**Evidence checkpoint - capture now:** Save the page observations and terminal output as `TC-12-01-in-memory-sliding-control.txt`.

**Pass checklist:** No exception occurs. The same Guid is reused inside the sliding window and is replaced after an idle period longer than the ten-second window.

## TC-05 - Relative absolute expiration

**Purpose:** Verify `ExpiresAfter="TimeSpan.FromSeconds(20)"`.

1. Open `http://localhost:5101/expiration?entry=tc05-01`.
2. Under **Relative absolute expiration**, copy Guid and Initialized UTC. Note the local clock time.
3. Before 20 seconds elapse from the first request, press **Ctrl+R**.
4. Confirm the relative-section Guid remains unchanged.

**Evidence checkpoint - capture now:** Save the reused relative section as `TC-05-01-before-expiry.png`.

5. Wait until at least 20 seconds have elapsed since the original request, not since the reload.
6. Press **Ctrl+R** once.
7. Confirm the relative-section Guid has changed and A logs a new child initialization.

**Evidence checkpoint - capture now:** Save the post-expiry section as `TC-05-02-after-expiry.png`. Copy the A initialization logs for both Guid values to `TC-05-03-expiry-terminal.txt`.

**Pass checklist:** The Guid is reused before the 20-second lifetime, then replaced after it expires.

## TC-06 - Absolute clock expiration

**Purpose:** Verify the `ExpiresOn` entry expires on its calculated clock deadline.

1. Open `http://localhost:5101/expiration?entry=tc06-01`.
2. Under **Absolute clock expiration**, copy Guid and Initialized UTC.
3. Calculate expiry as Initialized UTC plus 45 seconds.
4. Reload before that calculated expiry and confirm the clock-section Guid is unchanged.

**Evidence checkpoint - capture now:** Save the page showing the initial/reused clock section as `TC-06-01-before-clock-expiry.png`. In `TC-06-02-expiry-calculation.txt`, record initialized UTC, calculated expiry UTC, and your reload time.

5. Wait until after the calculated expiry UTC.
6. Reload once.
7. Confirm the clock-section Guid changes and A logs a new initialization.

**Evidence checkpoint - capture now:** Save the replaced clock section as `TC-06-03-after-clock-expiry.png`. Save matching A terminal lines as `TC-06-04-clock-expiry-terminal.txt`.

**Pass checklist:** The entry is reused before and replaced after the configured absolute clock expiry.

## TC-07 - Redis outage and recovery

**Purpose:** Separate local-cache behavior, a failed Redis-dependent request, and recovery without restarting applications.

1. Open `http://localhost:5101/?entry=tc07-warm-01`.
2. Confirm A displays a Guid and copy it.

**Evidence checkpoint - capture now:** Save as `TC-07-01-warm-A-page.png`; copy initial A log output to `TC-07-02-warm-A-terminal.txt`.

3. In a spare PowerShell terminal, run `docker compose stop redis`.
4. Return to Chrome and reload the unchanged `tc07-warm-01` URL on A.
5. Confirm whether the original local Guid is still displayed.

**Evidence checkpoint - capture now:** Save the outage local-entry page as `TC-07-03-outage-local-page.png` and A log output as `TC-07-04-outage-local-terminal.txt`.

6. Change the browser URL to `http://localhost:5101/?entry=tc07-new-01` and press **Enter**.
7. Record what the visitor receives and the A cache/provider logs.

**Evidence checkpoint - capture now:** Save the new-key response as `TC-07-05-outage-new-key-page.png`. Save all relevant HybridCache and distributed-provider output as `TC-07-06-outage-backend-terminal.txt`.

8. In the spare terminal, run `docker compose start redis` and wait for its confirmation.
9. Open `http://localhost:5101/?entry=tc07-recovered-01` and copy Guid.
10. Change only port `5101` to `5102` and load the exact recovered URL.
11. Compare B Guid to A Guid and inspect B terminal for child initialization.

**Evidence checkpoint - capture now:** Save A recovery page as `TC-07-07-recovery-A-page.png`, B recovery page as `TC-07-08-recovery-B-page.png`, and B request logs as `TC-07-09-recovery-B-terminal.txt`.

**Pass checklist:** Warm local output remains usable during outage; new backend-dependent request and its logging are recorded; after Redis recovery, B reuses the newly warmed A entry.

## TC-08 - Oversized payload

**Purpose:** Observe the 1 KB HybridCache payload limit independently from visitor response and provider failures.

1. Confirm `HybridCacheValidation\appsettings.json` still has `MaximumPayloadBytes = 1024` in `Program.cs`.
2. Open `http://localhost:5101/payload?entry=tc08-01`.
3. Confirm whether the page renders and copy **Oversized payload Guid**.
4. Inspect A terminal for HybridCache limit logging and any distributed-provider exception.

**Evidence checkpoint - capture now:** Save the page as `TC-08-01-first-payload-page.png`. Save the complete relevant A output as `TC-08-02-first-payload-terminal.txt`.

5. Reload the exact same URL and compare the Guid to step 3.
6. Open `http://localhost:5101/payload?entry=tc08-new-01` and record the result/logs.

**Evidence checkpoint - capture now:** Save the reload page as `TC-08-03-reload-page.png`, the new-key page as `TC-08-04-new-key-page.png`, and new-key A logs as `TC-08-05-new-key-terminal.txt`.

**Pass checklist:** The originally rendered response reaches the visitor. Record limit rejection, storage outcome, and any exception as separate observations; do not infer one from the other.

## TC-09 - Mixed-version output

**Purpose:** Record behavior when the cached markup differs by one visible word but both instances use the same Redis store.

1. Stop Instance B.
2. Copy the complete repository to a sibling folder, for example `D:\HybridCacheValidationChanged`.
3. In the copied `HybridCacheValidation\appsettings.json`, change only `Validation:VariantText` from `ORIGINAL` to `CHANGED`.
4. Start original A on 5101 as normal.
5. In the copied folder start B on 5102 with `--Validation:InstanceName=Instance-B`.
6. Open `http://localhost:5101/?entry=tc09-01`; record cached variant, Guid, and instance.

**Evidence checkpoint - capture now:** Save original configuration excerpt showing `ORIGINAL` as `TC-09-01-original-config.txt`. Save A page as `TC-09-02-original-A-page.png`.

7. Change browser port to 5102 with the same query key.
8. Record cached variant, Guid, and instance from B.
9. Copy both application terminal outputs for this flow.

**Evidence checkpoint - capture now:** Save changed configuration excerpt as `TC-09-03-changed-config.txt`, B page as `TC-09-04-changed-B-page.png`, and combined logs as `TC-09-05-A-B-terminal.txt`.

**Pass checklist:** Record actual observed output. Matching or different output is an observation, not automatically a pass/fail; include effective key configuration in the notes.

## TC-10 - Cache-layer timing

**Purpose:** Record cold, local-primary, and secondary-cache request timings without asserting a speed order.

1. In Chrome DevTools, click **Network**, confirm **Disable cache** and **Preserve log** are checked, then click clear.
2. Open `http://localhost:5101/?entry=tc10-01`.
3. In Network, click the document request for this URL and record its Duration/Time. Label it `cold`.
4. Confirm A logs `CachedGuid initialized`.

**Evidence checkpoint - capture now:** Capture the selected Network row/details and A terminal initialization line. Save image as `TC-10-01-cold-network.png` and logs as `TC-10-02-cold-terminal.txt`.

5. Clear Network. Reload the unchanged A URL.
6. Select the new document request and record Duration/Time. Label it `local primary hit`.
7. Confirm A has no new child initialization for this reload.

**Evidence checkpoint - capture now:** Save selected Network request as `TC-10-03-local-network.png` and A output as `TC-10-04-local-terminal.txt`.

8. Clear Network. Change the port from 5101 to 5102, keeping `?entry=tc10-01`.
9. Select B's document request and record Duration/Time. Label it `secondary-cache hit`.
10. Confirm B does not log child initialization for this request.

**Evidence checkpoint - capture now:** Save selected B Network request as `TC-10-05-secondary-network.png` and B terminal output as `TC-10-06-secondary-terminal.txt`.

**Pass checklist:** All three timings are recorded with their serving layer. No relative timing order is required.

## TC-11 - Browser Console cleanliness

**Purpose:** Verify normal CacheView flows do not produce unexpected browser Console errors or warnings.

1. Confirm Redis, Instance A on port 5101, and Instance B on port 5102 are running.
2. Open Chrome in an Incognito window and press **F12**.
3. Click the **Console** tab and click the clear-console icon.
4. Open `http://localhost:5101/?entry=tc11-01`.
5. Change only port `5101` to `5102`, keeping `?entry=tc11-01`, then load the page.
6. Open `http://localhost:5101/expiration?entry=tc11-exp-01`.
7. Open `http://localhost:5101/payload?entry=tc11-payload-01`.
8. Inspect Console after all requests. Do not include `/sliding`, because TC-04 intentionally creates an error response.

**Evidence checkpoint - capture now:** Save the Console after the normal requests as `TC-11-01-browser-console-clean.png`. If messages other than normal browser navigation entries appear, save them as `TC-11-02-browser-console-output.txt`.

**Pass checklist:** No unexpected application Console errors or warnings occur. Chrome `Navigated to` messages are normal informational entries.

## Result Recording Template

| Testcase | Result | Start UTC | Evidence files captured | Notes / issue classification |
|---|---|---|---|---|
| TC-01 | Pass | 2026-09-21 | TC-01 existing A/B evidence; TC-01 proxy screenshots; TC-01-01-proxy-first-response-terminal.txt; TC-01-02-proxy-second-instance-terminal.txt | Direct and proxy-routed requests reused Guid `c1e4b6d0-4b95-411b-9f09-79037a6d0972`. The proxy served Instance-B first, then Instance-A; only Instance-B initialized the child. |
| TC-02 | Pass | 2026-09-17 | TC-02-01-instance-a-terminal.txt; TC-02-02-instance-b-terminal.txt; TC-02-03-instance-a-after-restart-terminal.txt | Restarted Instance-A reused the unexpired shared entry without a new child initialization. |
| TC-03 | Pass | 2026-09-17 | TC-03-01-instance-a-terminal-before-restart.txt; TC-03-02-instance-b-terminal-before-restart.txt; TC-03-03-instance-a-terminal-after-restart.txt; TC-03-04-instance-b-terminal-after-restart.txt | Sequential restarts reused the entry; one cold child initialization was recorded. |
| TC-04 | Pass | 2026-09-21 | TC-04-05-server-exception.txt; TC-04-06-hybridcache-sliding-page-terminal.txt; refreshed browser exception screenshots | With HybridCache registered, `/sliding` threw `NotSupportedException` naming `ExpiresSliding` and directing use of `ExpiresAfter` or `ExpiresOn`. |
| TC-05 | Pass | 2026-09-21 | TC-05-04-terminal.txt; TC-05A-01-absolute-counterpart-initial.png; TC-05A-02-absolute-counterpart-before-expiry.png; TC-05A-03-absolute-counterpart-after-expiry.png; TC-05A-04-absolute-counterpart-terminal.txt | `ExpiresAfter` reused the value before expiry and rendered a replacement after expiry. The TC-05A counterpart used the same key, child, and 10-second lifetime as `/sliding`, and likewise reused before expiry and replaced after expiry. |
| TC-06 | Pass | 2026-09-17 | TC-06-04-absolute-clock-expiration-terminal.txt | `ExpiresOn` reused the value before the calculated deadline and rendered a replacement after it. |
| TC-07 | Pass | 2026-09-17 | TC-07-01-warm-a-page-terminal.txt; TC-07-02-outage-local-terminal.txt; TC-07-03-outage-backend-terminal.txt; TC-07-05-recovery-b-page-terminal.txt | Warm local output remained usable during Redis outage; backend failures were logged for a new key; sharing resumed after recovery. |
| TC-08 | Pass | 2026-09-17 | TC-08-01-first-payload-terminal.txt; TC-08-03-new-key-terminal.txt | The oversized response reached the visitor while HybridCache logged the maximum-payload rejection. |
| TC-09 | Observation recorded | 2026-09-17 | TC-09-03-original-a-terminal.txt; TC-09-04-changed-b-terminal.txt | Mixed-version output was recorded without pre-classifying the observed behavior as a defect. |
| TC-10 | Pass | 2026-09-17 | TC-10-01-cold-terminal.txt; TC-10-04-local-terminal.txt; TC-10-07-secondary-terminal.txt | Cold, local-cache-hit, and secondary-cache-hit timings were recorded as observations; no elapsed-time ordering was asserted. |
| TC-11 | Pass | 2026-09-17 | TC-11-01-browser-console-clean.png | Normal shared-cache, expiration, and payload requests produced only Chrome `Navigated to` messages; no application Console errors or warnings. |
| TC-12 | Pass | 2026-09-21 | TC-12-01-in-memory-first-response.png; TC-12-02-in-memory-within-window.png; TC-12-03-in-memory-after-idle-expiry.png; TC-12-04-in-memory-sliding-terminal.txt | Started with `Validation:UseHybridCache=false` at `http://localhost:5103/sliding`. The first Guid (`01288093-03a6-4af0-9365-f2e4cffc787e`) was reused within 10 seconds. After more than 10 seconds idle, a new Guid (`a20e3675-f6dd-49ff-899b-603fe402ea3e`) rendered. No exception occurred; the terminal recorded exactly two `CachedGuid initialized` events across the three requests. |