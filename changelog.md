---
layout: default
title: Changelog
changes:
  2019-09-01:
    - Fixed issues causing bulk update job to fail over the last few months.
    - Changed bulk update job to update flairs per-subreddit to allow better utilization of batch requests.
    - Flairs should now be updated every 3-4 days.
  2019-08-27:
    - Updated project to use Reddit.NET rather than a custom Reddit client. This will make maintenance and feature addition easier.
  2019-02-09:
    - Added SSL (HTTPS) to [flairs.championmains.com](https://flairs.championmains.com/).
  2019-02-02:
    - New beta UI now available with updates for season 9 ranks. Works on mobile as well.
  2019-02-01:
    - Fix issues with summoner registration and updating caused by the removal of the V3 Riot API.
  2018-12-18:
    - Hourly update job has been fixed. Flairs should now be updated every 3 days.
  2018-12-02:
    - Hourly update job will soon (by the new year) be reworked completely for long-term robustness.
  2018-04-12:
    - The hourly update job is experiencing intermittent failures.
  2018-02-21:
    - The hourly update job is running again, but at a lower capacity. On average, summoners will be updated every 7 days.
  2018-02-15:
    - Updated Pyrobot to use the [Camille](https://github.com/MingweiSamuel/Camille) Riot API library.
  2017-12-18:
    - Patched a bug that prevented users from registering existing accounts that had undergone a name change.
  2017-08-30:
    - The hourly update job has been disabled to due changes in the Riot API. There is currently no estimate of when it will be
      re-enabled
  2017-07-22:
    - Updated the hourly update job to also update subreddit flair CSS. Previously flair CSS would update sporadically and new CSS
      could take several hours to show up on new subreddits.
  2017-07-17:
    - Flairs now show a preview in both the subreddit table and the new edit modal.
  2017-07-15:
    - Updated backend for the V3 Riot API changes.
    - Summoners can now be added even if they are already registered with another Reddit account. This change is to help with many people
      who have lost access to their old Reddit accounts and can't re-register their summoners.
  2017-03-03:
    - Added some APIs for public use. If you are interested in pulling data from ChampionMains Flairs, contact [/u/LugnutsK](https://reddit.com/u/LugnutsK).
  2016-12-17:
    - The hourly update job has been failing since around 2016-11-11. A rewrite and fix are in progress. If you want to manually update your flair, you can go to
      [flairs.championmains.com](http://flairs.championmains.com/) and press the refresh buttons next to each summoner and the green checkmarks next to each subreddit.
  2016-11-23:
    - There has been some downtime over the past few days (403 not available). Hopefully some changes today will help stop that from happening so often.
  2016-11-15:
    - Fixed a bug causing updates of summoners ranked in flex queue to fail.
  2016-09-29:
    - Fixed configuration setting that was causing the hourly update job to not run after site was moved on 2016/09/24.
  2016-09-28:
    - Removed prestige progress from subreddits that don't have prestige enabled in the website UI.
  2016-09-26:
    - Updated the navigation of [docs.championmains.com](http://docs.championmains.com/) and added a [support page](support).
  2016-09-24:
    - Moved site to [flairs.championmains.com](http://flairs.championmains.com/), and this documentation site to [docs.championmains.com](http://docs.championmains.com/).
  2016-09-23:
    - Fixed an issue that was causing the hourly update job to fail after 2016-09-20.
  2016-09-10:
    - Fixed an issue causing hourly update job to sometimes fail.
  2016-09-07:
    - Fixed an issue causing hourly update job to sometimes fail.
  2016-09-02:
    - Fixed several issues that were causing flairs to behave weirdly and/or not automatically update. Issues
      have been occuring over the past week or so.
  2016-08-27:
    - Added exact champion mastery text.
    - Added prestiges up to 5 million (previous max was 2 million).
    - Changed UI to hide buttons instead of greying them out.
  2016-08-25:
    - Added changelog to documentation site (this page).
    - Fixed bug causing website to display wrong user flair information after internal changes on 2016-08-12.
  2016-08-22:
    - Fixed crashes in the hourly updater.
  2016-08-14:
    - Changed website UI to show when data was being loaded, and prevent button spamming.
    - Fixed crashes in the hourly updater.
  2016-08-15:
    - Fixed crashes causing summoner info to sometimes not update.
  2016-07-30:
    - Fixed bug causing flair text with special characters to behave strangely.
  2016-07-28:
    - Fixed bug causing subreddits to not be in order of champion mastery.
    - Fixed bug causing duplicate summoners, doubling or tripling peoples' mastery points.
  2016-07-27:
    - Fixed bug causing hourly updater to clear users' flair text.
  2016-07-26:
    - Summoner and flair updates temporarily not working.
    - Misc. changes and fixes.
  2016-07-22:
    - Initial-ish public release.
---

{% for dateChanges in page.changes %}
### {{ dateChanges[0] }}
{% for change in dateChanges[1] %}
- {{ change }}
{% endfor %}
{% endfor %}
