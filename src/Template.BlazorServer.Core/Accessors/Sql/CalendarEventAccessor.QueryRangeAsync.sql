SELECT
    *
FROM
    CalendarEvent
WHERE
    ((AllDay = 0) AND (StartAt < /*@ endAt */'') AND (EndAt > /*@ startAt */''))
    OR ((AllDay = 1) AND (StartAt < /*@ endDate */'') AND (EndAt > /*@ startDate */''))
ORDER BY
    StartAt, Id
