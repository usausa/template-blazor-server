UPDATE
    CalendarEvent
SET
    CalendarId = /*@ calendarId */0,
    Title = /*@ title */'',
    StartAt = /*@ startAt */'',
    EndAt = /*@ endAt */'',
    AllDay = /*@ allDay */0,
    Description = /*@ description */'',
    Version = Version + 1
WHERE
    Id = /*@ id */0
    AND ((/*@ version */0 IS NULL) OR (Version = /*@ version */0))
RETURNING
    Version
