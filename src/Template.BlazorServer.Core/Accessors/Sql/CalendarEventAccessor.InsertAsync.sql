INSERT INTO
    CalendarEvent (CalendarId, Title, StartAt, EndAt, AllDay, Description, Version, CreatedAt)
VALUES
    (/*@ calendarId */0, /*@ title */'', /*@ startAt */'', /*@ endAt */'', /*@ allDay */0, /*@ description */'', 1, /*@ createdAt */'')
RETURNING
    Id
