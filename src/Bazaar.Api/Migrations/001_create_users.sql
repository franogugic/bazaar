CREATE TABLE users(
    id                  INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY, 
    username            VARCHAR(30) NOT NULL,
    email               VARCHAR(254) NOT NULL,
    password_hash       TEXT NOT NULL,
    city                VARCHAR(50),
    country             VARCHAR(50),
    created_at          TIMESTAMPTZ NOT NULL DEFAULT now(),

    CONSTRAINT user_username_length CHECK(char_length(username) > 3),
    CONSTRAINT user_email_format CHECK(position('@' in email) > 1)                  
);

CREATE UNIQUE INDEX user_username_unique ON users (lower(username));
CREATE UNIQUE INDEX user_email_unique ON users(lower(email));

