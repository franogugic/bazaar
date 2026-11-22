CREATE TABLE follows (
    follower_id INT         NOT NULL,
    followed_id INT         NOT NULL,
    created_at  TIMESTAMPTZ NOT NULL DEFAULT NOW(),

    CONSTRAINT follows_pk          PRIMARY KEY (follower_id, followed_id),
    CONSTRAINT follows_follower_fk FOREIGN KEY (follower_id) REFERENCES users (id),
    CONSTRAINT follows_followed_fk FOREIGN KEY (followed_id) REFERENCES users (id),
    CONSTRAINT follows_not_self    CHECK (follower_id <> followed_id)
);
