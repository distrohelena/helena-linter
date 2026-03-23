class ControlBodyBracesInvalid {
    void test(boolean flag, int value, Object lock) {
        if (flag)
            work();
        else if (value > 0)
            work();
        else
            work();

        for (int i = 0; i < value; i++)
            work();

        for (String item : new String[] {"a"})
            work();

        while (flag)
            work();

        do
            work();
        while (flag);
    }

    void work() {
    }
}
