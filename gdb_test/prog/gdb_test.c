int test();

void main()
{
  int *a = (int*)0x100;
  (*a)++;
  *a += test();

  int *b = (int*)0x200;
  (*b)++;

  while(1);
}

int test()
{
    return 3;
}

