-- Run in the same schema as HocSinh, DeThi and KetQuaThi. No existing data is changed.
CREATE TABLE IF NOT EXISTS study_giam_sat_thi (
    HocSinhID integer NOT NULL,
    DeThiID integer NOT NULL,
    SoLan integer NOT NULL DEFAULT 0 CHECK (SoLan BETWEEN 0 AND 3),
    LanCuoi timestamp with time zone,
    PRIMARY KEY (HocSinhID, DeThiID)
);
