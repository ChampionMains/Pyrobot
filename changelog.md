---
layout: default
title: Changelog
changes:
  2016-09-07:
    - Fixed issue causing hourly update job to sometimes fail.
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
