using Bogus;
using Dapper;
using Newtonsoft.Json;
using Npgsql;

namespace gettingstarted.week34.prg_1_Dapper;


public static class Helper
{
    public static string Mode = "My Solution";
    
    public static readonly Uri Uri = new Uri(Environment.GetEnvironmentVariable("pgconn")!);

    public static readonly string
        ProperlyFormattedConnectionString = string.Format(
            "Server={0};Database={1};User Id={2};Password={3};Port={4};Pooling=true;MaxPoolSize=3",
            Uri.Host,
            Uri.AbsolutePath.Trim('/'),
            Uri.UserInfo.Split(':')[0],
            Uri.UserInfo.Split(':')[1],
            Uri.Port > 0 ? Uri.Port : 5432);

    public static readonly NpgsqlDataSource PostgresDockerDataSource = new NpgsqlDataSourceBuilder(
        "Server=localhost;Database=postgres;User Id=postgres;Password=postgres;Port=5432;Pooling=true;MaxPoolSize=10;"
    ).Build();


    public static readonly NpgsqlDataSource DataSource =
        new NpgsqlDataSourceBuilder(ProperlyFormattedConnectionString).Build();

    public static void TriggerRebuild()
    {
        using (var conn = DataSource.OpenConnection())
        {
            conn.Execute(RebuildScript);
        }
    }

    public static string MyBecause(object actual, object expected)
    {
        string expectedJson = JsonConvert.SerializeObject(expected, Formatting.Indented);
        string actualJson = JsonConvert.SerializeObject(actual, Formatting.Indented);

        return $"because we want these objects to be equivalent:\nExpected:\n{expectedJson}\nActual:\n{actualJson}";
    }

    public static Book MakeRandomBookWithId(int id)
    {
        return new Faker<Book>()
            .RuleFor(b => b.BookId, id)
            .RuleFor(b => b.Publisher, p => p.Company.CompanyName())
            .RuleFor(b => b.Title, t => t.Lorem.Sentence())
            .RuleFor(b => b.CoverImgUrl, "https://picsum.photos/200/300")
            .Generate();
    }

    public static EndUser MakeRandomUserWithId(int id)
    {
        return new Faker<EndUser>()
            .RuleFor(u => u.EndUserId, id)
            .RuleFor(u => u.PasswordHash, h => h.Random.Word())
            .RuleFor(u => u.Salt, h => h.Random.Word())
            .RuleFor(u => u.Email, e => e.Person.Email)
            .RuleFor(u => u.ProfileImgUrl, pr => pr.Random.Int(50))
            .RuleFor(u => u.Status, st => st.Random.Word())
            .RuleFor(u => u.Role, r => r.Random.Word())
            .Generate();
    }

    public static Author MakeRandomAuthorWithId(int id)
    {
        return new Faker<Author>()
            .RuleFor(a => a.AuthorId, id)
            .RuleFor(a => a.Bithday, b => b.Date.Recent())
            .RuleFor(a => a.Name, n => n.Name.FullName())
            .RuleFor(a => a.Nationality, n => n.Lorem.Word())
            .Generate();
    }

    public static string RebuildScript = $@"DROP SCHEMA IF EXISTS library CASCADE;
CREATE SCHEMA library;
create sequence library.books_id_seq;

create sequence library.authors_id_seq;

create sequence library.user_id_seq;

create sequence library.books_book_id_seq
    as integer;

create sequence library.authors_author_id_seq
    as integer;

create sequence library.end_users_end_user_id_seq
    as integer;

create sequence library.books_book_id_seq1
    as integer;

create sequence library.authors_author_id_seq1
    as integer;

create sequence library.end_users_end_user_id_seq1
    as integer;

create table if not exists library.books
(
    book_id       integer generated by default as identity,
    title         text,
    publisher     text,
    cover_img_url text,
    primary key (book_id)
);

alter sequence library.books_id_seq owned by library.books.book_id;

create index if not exists books_id_title_index
    on library.books (book_id, title);

create table if not exists library.authors
(
    author_id   integer generated by default as identity,
    name        text,
    birthday    date,
    nationality text,
    primary key (author_id)
);

alter sequence library.authors_id_seq owned by library.authors.author_id;

create index if not exists authors_author_id_name_index
    on library.authors (author_id, name);

create table if not exists library.author_wrote_book_items
(
    book_id   integer not null,
    author_id integer not null,
    constraint compositekey
        primary key (book_id, author_id),
    constraint fk_book
        foreign key (book_id) references library.books
            on delete cascade,
    constraint fk_author
        foreign key (author_id) references library.authors
            on delete cascade
);

create table if not exists library.end_users
(
    email           text not null,
    status          text,
    end_user_id     integer generated always as identity,
    password_hash   text not null,
    salt            text not null,
    role            text,
    profile_img_url integer,
    constraint user_pk
        primary key (end_user_id)
);

alter sequence library.user_id_seq owned by library.end_users.end_user_id;

create table if not exists library.reading_list_items
(
    user_id integer not null,
    book_id integer not null,
    constraint reading_list_items_pk
        primary key (book_id, user_id),
    constraint readinglistitems_user_id_fk
        foreign key (user_id) references library.end_users
            on delete cascade,
    constraint readinglistitems_books_id_fk
        foreign key (book_id) references library.books
            on delete cascade
); ";
}